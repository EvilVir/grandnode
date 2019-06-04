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

        public ReservationsController(IProductReservationService productReservationService, IProductService productService, IOrderService orderService, ILanguageService languageService)
        {
            this._productReservationService = productReservationService;
            this._productService = productService;
            this._orderService = orderService;
            this._languageService = languageService;
        }

        public async Task<IActionResult> GetReservations(DateTime from, DateTime? to, string language)
        {
            var output = new List<ReservationData>();
            var reservableProducts = (await _productService.GetProductsOfType(ProductType.Reservation)).ToDictionary(k => k.Id, v => v);
            var orders = await _orderService.SearchOrders(createdFromUtc: from.ToUniversalTime(), createdToUtc: to?.ToUniversalTime(), os: OrderStatus.Complete);
            var languageId = (await _languageService.GetAllLanguages()).Where(x => x.LanguageCulture == language || x.LanguageCulture.StartsWith(language)).Select(x => x.Id).FirstOrDefault();

            foreach (var order in orders)
            {
                var reservedSlots = await _productReservationService.GetProductReservationsByOrderId(order.Id);

                if (reservedSlots.Any())
                {
                    var outputData = new ReservationData() {
                        Code = order.ShortId,
                        Date = order.CreatedOnUtc,
                        LastUpdate = order.CreatedOnUtc,
                    };

                    var groupedSlots = reservedSlots.GroupBy(x => x.ProductId)
                                                    .ToDictionary(k => k.Key, v => v.GroupBy(x => x.Resource).ToDictionary(k2 => k2.Key, v2 => new { StartDate = v2.Min(x => x.Date), EndDate = v2.Max(x => x.Date) }));

                    foreach (var productGroup in groupedSlots)
                    {
                        if (reservableProducts.ContainsKey(productGroup.Key))
                        {
                            var product = reservableProducts[productGroup.Key];
                            var productName = product.GetLocalized(x => x.Name, languageId);

                            foreach (var resourceGroup in productGroup.Value)
                            {
                                var resource = product.Resources.Where(x => x.SystemName == resourceGroup.Key).FirstOrDefault();

                                if (resource != null)
                                {
                                    var outputItem = new ReservationItemData() {
                                        Code = resource.SystemName,
                                        Name = $"{productName} {resource.Name}",
                                        Latitude = resource.Latitude,
                                        Longitude = resource.Longitude,
                                        Quantity = 1,
                                        Start = resourceGroup.Value.StartDate.Add(product.ReservationStartDelta),
                                        End = resourceGroup.Value.EndDate.Add(product.ReservationEndDelta),
                                        MaxUses = null,
                                    };

                                    outputData.Items.Add(outputItem);
                                }
                            }
                        }
                    }

                    if (outputData.Items.Any())
                    {
                        outputData.Start = outputData.Items.Where(x => x.Start != null).Min(x => x.Start.Value);
                        outputData.End = outputData.Items.Where(x => x.End != null).Max(x => x.End.Value);
                        output.Add(outputData);
                    }
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