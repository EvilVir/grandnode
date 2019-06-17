using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services
{
    public class ReservationsCalendarPublishingService
    {
        private readonly IProductService _productService;
        private readonly IProductReservationService _productReservationService;
        private readonly IOrderService _orderService;
        private readonly ILanguageService _languageService;
        private readonly IProductAttributeParser _productAttributeParser;

        public ReservationsCalendarPublishingService(IProductReservationService productReservationService, IProductService productService, IOrderService orderService, ILanguageService languageService, IProductAttributeParser productAttributeParser)
        {
            this._productReservationService = productReservationService;
            this._productService = productService;
            this._orderService = orderService;
            this._languageService = languageService;
            this._productAttributeParser = productAttributeParser;
        }

        public async Task<Calendar> GetCalendar(string productId, string resourceId, int daysOffset, string language)
        {
            var output = new Calendar();

            var dateFrom = DateTime.UtcNow.AddDays(-daysOffset);
            var reservableProducts = (await _productService.GetProductsOfType(ProductType.Reservation)).ToDictionary(k => k.Id, v => v);
            var orders = await _orderService.SearchOrders(anyReservationItemFromUtc: dateFrom.ToUniversalTime(), nos: OrderStatus.Cancelled);
            var languageId = (await _languageService.GetAllLanguages()).Where(x => x.LanguageCulture == language || x.LanguageCulture.StartsWith(language)).Select(x => x.Id).FirstOrDefault();

            foreach (var order in orders)
            {
                var orderSlots = (await _productReservationService.GetProductReservationsByOrderId(order.Id)).GroupBy(x => x.OrderItemId).ToDictionary(k => k.Key, v => v.ToList());

                foreach (var orderItem in order.OrderItems.Where(x => x.ProductId == productId && reservableProducts.ContainsKey(x.ProductId)))
                {
                    if (!orderSlots.ContainsKey(orderItem.Id))
                    {
                        continue;
                    }

                    var orderItemSlots = orderSlots[orderItem.Id];
                    var resourcesSlots = orderItemSlots.GroupBy(x => x.Resource).Where(x => string.IsNullOrEmpty(resourceId) || x.Key == resourceId).ToDictionary(k => k.Key, v => v.ToList());

                    if (!resourcesSlots.Any())
                    {
                        continue;
                    }

                    var product = reservableProducts[orderItem.ProductId];
                    var productName = product.GetLocalized(x => x.Name, languageId);
                    var orderItemAttributes = _productAttributeParser.ParseProductAttributeValues(product, orderItem.AttributesXml).Select(x => x.GetLocalized(y => y.Name, languageId)).ToArray();

                    var calendarEvent = new CalendarEvent() {
                        Summary = $"Order {orderItem.Id}",
                        Location = resourceId,
                        Uid = orderItem.Id,
                        DtStamp = new CalDateTime(order.CreatedOnUtc),
                        LastModified = new CalDateTime(order.UpdatedOnUtc),
                        Start = null,
                        End = null
                    };

                    foreach (var resourceSlots in resourcesSlots)
                    {
                        var resource = product.Resources.Where(x => x.SystemName == resourceSlots.Key).FirstOrDefault();
                        var start = resourceSlots.Value.Min(x => x.Date).Add(product.ReservationStartDelta);
                        var end = resourceSlots.Value.Max(x => x.Date).AddDays(!product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day ? 1 : 0).Add(product.ReservationEndDelta);

                        if (calendarEvent.Start == null || calendarEvent.Start.Date > start)
                        {
                            calendarEvent.Start = new CalDateTime(start);
                        }

                        if (calendarEvent.End == null || calendarEvent.End.Date < end)
                        {
                            calendarEvent.End = new CalDateTime(end);
                        }

                        calendarEvent.Summary += (!string.IsNullOrEmpty(calendarEvent.Summary) ? ", " : "") + resource.SystemName;
                        calendarEvent.Uid += $"_{resource.SystemName}";
                    }

                    output.Events.Add(calendarEvent);
                }
            }

            return output;
        }
    }
}
