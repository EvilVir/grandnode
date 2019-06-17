using Grand.Core.Configuration;
using System.Collections.Generic;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization
{
    public class ReservationsSynchronizationSettings : ISettings
    {
        public class ExternalCalendarSettings
        {
            public string Id { get; set; }
            public string ProductId { get; set; }
            public string ResourceSystemName { get; set; }
            public string Url { get; set; }
        }

        public class PublishedCalendarSettings
        {
            public string Id { get; set; }
            public string ProductId { get; set; }
            public string ResourceSystemName { get; set; }
            public int PastDays { get; set; }
        }

        public List<ExternalCalendarSettings> ExternalCalendars { get; set; } = new List<ExternalCalendarSettings>();

        public List<PublishedCalendarSettings> PublishedCalendars { get; set; } = new List<PublishedCalendarSettings>();
    }
}
