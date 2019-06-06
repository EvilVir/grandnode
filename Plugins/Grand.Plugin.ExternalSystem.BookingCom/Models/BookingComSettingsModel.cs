using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Plugin.ExternalSystem.BookingCom.Models
{
    public class BookingComSettingsModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BookingCom.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BookingCom.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.BookingCom.Fields.ScheduledTaskInterval")]
        public int ScheduledTaskInterval { get; set; }
    }
}
