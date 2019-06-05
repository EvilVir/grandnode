using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Payments.DotPay.Models
{
    public class DotPaySettingsModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.DotPay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.DotPay.Fields.ShopId")]
        public int ShopId { get; set; }
        public bool ShopId_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.DotPay.Fields.Pin")]
        public string Pin { get; set; }
        public bool Pin_OverrideForStore { get; set; }
    }
}
