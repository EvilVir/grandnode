using Grand.Core.Configuration;
using System;

namespace Grand.Plugin.ExternalSystem.BookingCom
{
    public class BookingComSettings : ISettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? LastCheck { get; set; }
    }
}
