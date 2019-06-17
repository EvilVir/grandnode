using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Stores;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Ical.Net;
using Ical.Net.CalendarComponents;
using shortid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services
{
    public class ReservationsSynchronizationService
    {
        protected const string GA_EXTERNAL_SOURCE_KEY = "ExternalSourceId";
        protected const string GA_EXTERNAL_ID_KEY = "ExternalId";
        protected const string GUEST_CUSTOMER_CACHE_KEY = "ReservationsSynchronizationGuestCustomer_{0}";

        protected class ProductReservationIdEqualityComparer : IEqualityComparer<ProductReservation>
        {
            public bool Equals(ProductReservation x, ProductReservation y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(ProductReservation obj)
            {
                return obj?.Id.GetHashCode() ?? 0;
            }
        }

        protected readonly ISettingService _settingService;
        protected readonly IStoreService _storeService;
        protected readonly IOrderService _orderService;
        protected readonly IRepository<Order> _orderRepository;
        protected readonly IProductService _productService;
        protected readonly IProductReservationService _productReservationService;
        protected readonly ICustomerService _customerService;
        protected readonly ICacheManager _cacheManager;
        protected readonly ILogger _logger;

        public ReservationsSynchronizationService(
            IStoreService storeService,
            ISettingService settingService,
            IOrderService orderService,
            IRepository<Order> orderRepository,
            IProductService productService,
            IProductReservationService productReservationService,
            ICustomerService customerService,
            ICacheManager cacheManager,
            ILogger logger)
        {
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._orderRepository = orderRepository;
            this._productService = productService;
            this._productReservationService = productReservationService;
            this._customerService = customerService;
            this._cacheManager = cacheManager;
            this._logger = logger;
        }

        public async Task SynchronizeExternalCalendars()
        {
            var stores = await _storeService.GetAllStores();

            foreach (var store in stores)
            {
                var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(store.Id);

                if (settings.ExternalCalendars != null)
                {
                    foreach (var externalCalendar in settings.ExternalCalendars)
                    {
                        await SynchronizeExternalCalendar(externalCalendar.Url, externalCalendar.ProductId, externalCalendar.ResourceSystemName, store);
                    }
                }
            }
        }

        public async Task SynchronizeExternalCalendar(string url, string productId, string resourceSystemName, Store store)
        {
            var externalSourceHost = new Uri(url).Host;
            var product = await _productService.GetProductById(productId);
            var guestCustomer = await GetGuestCustomer(store);

            if (product.ProductType != ProductType.Reservation || !product.Resources.Any(x => x.SystemName == resourceSystemName))
            {
                return;
            }

            var calendar = await DownloadCalendar(url);
            foreach (var calendarEvent in calendar.Events)
            {
                var genericAttributes = new List<GenericAttribute>()
                {
                    new GenericAttribute() { Key = GA_EXTERNAL_SOURCE_KEY, Value = url.HashMD5() },
                    new GenericAttribute() { Key = GA_EXTERNAL_ID_KEY, Value = $"{calendarEvent.Uid}:{calendarEvent.Start.Ticks}:{calendarEvent.End.Ticks}".HashMD5() }
                };

                var orderExists = await _orderRepository.CheckIfExistsByGenericAttributesAsync(genericAttributes);

                if (!orderExists)
                {
                    var sdate = calendarEvent.Start.Date;
                    var edate = calendarEvent.End.Date.AddDays(product.IntervalUnitType == IntervalUnit.Day && !product.IncBothDate ? -1 : 0);

                    var order = new Order {
                        StoreId = store.Id,
                        CustomerId = guestCustomer.Id,
                        CustomerEmail = guestCustomer.Email,
                        FirstName = calendarEvent.Summary ?? calendarEvent.Uid,
                        LastName = externalSourceHost,
                        OrderGuid = Guid.NewGuid(),
                        RefundedAmount = decimal.Zero,
                        OrderStatus = OrderStatus.Complete,
                        PaymentStatus = PaymentStatus.Paid,
                        PaidDateUtc = null,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                        GenericAttributes = genericAttributes,
                        BillingAddress = new Address() {
                            Email = guestCustomer.Email,
                            FirstName = calendarEvent.Summary ?? calendarEvent.Uid,
                            LastName = externalSourceHost
                        }
                    };

                    var orderItem = new OrderItem() {
                        ProductId = productId,
                        ShortId = ShortId.Generate(true, false, 7).ToUpperInvariant(),
                        OrderItemGuid = Guid.NewGuid(),
                        Quantity = 1,
                        RentalStartDateUtc = sdate.Add(product.ReservationStartDelta),
                        RentalEndDateUtc = edate.Add(product.ReservationEndDelta),
                        CreatedOnUtc = DateTime.UtcNow,
                    };

                    order.OrderItems.Add(orderItem);

                    await _orderService.InsertOrder(order);

                    var reservationSlots = await _productReservationService.GetProductReservationsByProductId(productId, null, sdate, edate, resourceSystemName: resourceSystemName);

                    foreach (var slot in reservationSlots)
                    {
                        if (!string.IsNullOrEmpty(slot.OrderId))
                        {
                            _logger.Error($"Reservation slot {slot.Id} ({productId} :: {resourceSystemName}) is already occupied by order {slot.OrderId}, item {slot.OrderItemId}. Can't mark it for {order.Id}, item {orderItem.Id}.");
                            continue;
                        }

                        slot.OrderId = order.Id;
                        slot.OrderItemId = orderItem.Id;
                    }

                    await _productReservationService.UpdateProductReservation(reservationSlots);


                    var note = new OrderNote() {
                        OrderId = order.Id,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        CreatedByCustomer = false,
                        Note = $"{calendarEvent.Location} {calendarEvent.Description} {calendarEvent.Summary}"
                    };

                    await _orderService.InsertOrderNote(note);
                }
            }
        }

        protected async Task<Customer> GetGuestCustomer(Store store)
        {
            var key = string.Format(GUEST_CUSTOMER_CACHE_KEY, store.Id);
            return await _cacheManager.GetAsync(key, async () =>
            {

                var output = await _customerService.GetCustomerBySystemName(ReservationsSynchronizationPlugin.GUEST_CUSTOMER_SYSTEM_NAME);

                if (output == null)
                {
                    output = await _customerService.InsertGuestCustomer(store, "", ReservationsSynchronizationPlugin.GUEST_CUSTOMER_SYSTEM_NAME, store.CompanyEmail);
                }

                return output;
            });
        }

        protected async Task<Calendar> DownloadCalendar(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            request.Headers.Add(HttpRequestHeader.Accept, "*/*");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    return Calendar.Load(stream);
                }
            }
        }
    }
}
