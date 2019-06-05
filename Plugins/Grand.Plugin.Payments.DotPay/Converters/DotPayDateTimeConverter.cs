using Newtonsoft.Json.Converters;

namespace Grand.Plugin.Payments.DotPay.Converters
{
    public class DotPayDateTimeConverter : IsoDateTimeConverter
    {
        public DotPayDateTimeConverter()
        {
            this.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }
}
