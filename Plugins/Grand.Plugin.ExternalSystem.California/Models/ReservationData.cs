using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Grand.Plugin.ExternalSystem.California.Models
{
    public class ReservationData
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("last_update")]
        public DateTime LastUpdate { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("items")]
        public List<ReservationItemData> Items { get; set; } = new List<ReservationItemData>();
    }
}
