using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Grand.Plugin.ExternalSystem.California.Models
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReservationItemData
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public long Quantity { get; set; }

        public long? MaxUses { get; set; } = null;
    }
}
