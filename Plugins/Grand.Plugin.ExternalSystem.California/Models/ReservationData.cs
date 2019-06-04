using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Grand.Plugin.ExternalSystem.California.Models
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReservationData
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime LastUpdate { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<ReservationItemData> Items { get; set; } = new List<ReservationItemData>();
    }
}
