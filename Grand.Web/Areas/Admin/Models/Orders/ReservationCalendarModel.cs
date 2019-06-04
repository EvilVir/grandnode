using Grand.Framework.Mvc.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReservationCalendarModel : BaseGrandModel
    {
        public string Title => $"[{ResourceDescription}] {CustomerDescription}";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ResourceId { get; set; }
        public string ResourceDescription { get; set; }
        public string Color { get; set; }
        public string OrderId { get; set; }
        public string OrderLink { get; set; }
        public string CustomerId { get; set; }
        public string CustomerDescription { get; set; }
        public string CustomerLink { get; set; }
        public string AdditionalServicesDescription { get; set; }
    }
}
