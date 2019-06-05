using Grand.Core.Configuration;

namespace Grand.Plugin.Payments.DotPay
{
    public class DotPaySettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public int ShopId { get; set; }
        public string Pin { get; set; }
    }
}
