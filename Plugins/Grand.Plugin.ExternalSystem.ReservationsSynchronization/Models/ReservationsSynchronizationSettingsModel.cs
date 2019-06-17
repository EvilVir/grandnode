using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using MongoDB.Bson;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Models
{
    [Validator(typeof(ReservationsSynchronizationSettingsModelValidator))]
    public class ReservationsSynchronizationSettingsModel : BaseGrandModel
    {
        [Validator(typeof(ReservationsSynchronizationSettingsModelValidator.ExternalCalendarModelValidator))]
        public class ExternalCalendarModel
        {
            public string Id { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId")]
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ResourceSystemName")]
            public string ResourceSystemName { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url")]
            public string Url { get; set; }
        }

        [Validator(typeof(ReservationsSynchronizationSettingsModelValidator.PublishedCalendarModelValidator))]
        public class PublishedCalendarModel
        {
            public string Id { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId")]
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ResourceSystemName")]
            public string ResourceSystemName { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.Url")]
            public string Url { get; set; }

            [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.PastDays")]
            public int PastDays { get; set; } = 7;
        }

        [GrandResourceDisplayName("Plugins.ExternalSystem.ReservationsSynchronization.Fields.SynchronizationInterval")]
        public int SynchronizationInterval { get; set; }
    }
}
