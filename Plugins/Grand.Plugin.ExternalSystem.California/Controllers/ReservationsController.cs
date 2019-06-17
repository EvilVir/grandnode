using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Plugin.ExternalSystem.California.Models;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.California.Controllers
{
    public class ReservationsController : BasePluginController
    {
        private readonly IProductService _productService;
        private readonly IProductReservationService _productReservationService;
        private readonly IOrderService _orderService;
        private readonly ILanguageService _languageService;
        private readonly IProductAttributeParser _productAttributeParser;

        public ReservationsController(IProductReservationService productReservationService, IProductService productService, IOrderService orderService, ILanguageService languageService, IProductAttributeParser productAttributeParser)
        {
            this._productReservationService = productReservationService;
            this._productService = productService;
            this._orderService = orderService;
            this._languageService = languageService;
            this._productAttributeParser = productAttributeParser;
        }

        public async Task<IActionResult> GetReservations(DateTime dateFrom, DateTime? dateTo, string language)
        {
            var output = new List<ReservationData>();
            var reservableProducts = (await _productService.GetProductsOfType(ProductType.Reservation)).ToDictionary(k => k.Id, v => v);
            var orders = await _orderService.SearchOrders(anyReservationItemFromUtc: dateFrom.ToUniversalTime(), anyReservationItemToUtc: dateTo?.ToUniversalTime(), nos: OrderStatus.Cancelled);
            var languageId = (await _languageService.GetAllLanguages()).Where(x => x.LanguageCulture == language || x.LanguageCulture.StartsWith(language)).Select(x => x.Id).FirstOrDefault();

            foreach (var order in orders)
            {
                var orderSlots = (await _productReservationService.GetProductReservationsByOrderId(order.Id)).GroupBy(x => x.OrderItemId).ToDictionary(k => k.Key, v => v.ToList());

                foreach (var orderItem in order.OrderItems.Where(x => reservableProducts.ContainsKey(x.ProductId)))
                {
                    if (!orderSlots.ContainsKey(orderItem.Id))
                    {
                        continue;
                    }

                    var orderItemSlots = orderSlots[orderItem.Id];
                    var resourcesSlots = orderItemSlots.GroupBy(x => x.Resource).ToDictionary(k => k.Key, v => v.ToList());
                    var product = reservableProducts[orderItem.ProductId];
                    var productName = product.GetLocalized(x => x.Name, languageId);
                    var orderItemAttributes = _productAttributeParser.ParseProductAttributeValues(product, orderItem.AttributesXml).Select(x => x.GetLocalized(y => y.Name, languageId)).ToArray();

                    var outputData = new ReservationData() {
                        Code = orderItem.ShortId,
                        Date = order.CreatedOnUtc,
                        LastUpdate = order.UpdatedOnUtc,
                    };

                    foreach (var resourceSlots in resourcesSlots)
                    {
                        var resource = product.Resources.Where(x => x.SystemName == resourceSlots.Key).FirstOrDefault();

                        var outputItem = new ReservationItemData() {
                            Code = resource.SystemName,
                            Name = $"{productName} {resource.Name}",
                            Latitude = resource.Latitude,
                            Longitude = resource.Longitude,
                            Quantity = 1,
                            Start = resourceSlots.Value.Min(x => x.Date).Add(product.ReservationStartDelta),
                            End = resourceSlots.Value.Max(x => x.Date).AddDays(!product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day ? 1 : 0).Add(product.ReservationEndDelta),
                            MaxUses = null,
                        };

                        outputData.Items.Add(outputItem);
                    }

                    outputData.Start = outputData.Items.Where(x => x.Start != null).Min(x => x.Start.Value);
                    outputData.End = outputData.Items.Where(x => x.End != null).Max(x => x.End.Value);

                    output.Add(outputData);
                }
            }

            return await Task.FromResult(Json(output));
        }

        public async Task<IActionResult> BC_GetReservations(string from, string to, string lang = "pl")
        {
            return await GetReservations(from != null ? DateTime.ParseExact(from, "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.Now, to != null ? DateTime.ParseExact(to, "yyyyMMdd", CultureInfo.InvariantCulture) : (DateTime?)null, lang);
        }
    }
}