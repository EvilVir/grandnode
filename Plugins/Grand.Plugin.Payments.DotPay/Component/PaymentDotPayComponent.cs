using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Payments.DotPay.Component
{
    [ViewComponent(Name = "PaymentDotPay")]
    public class PaymentDotPayComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.DotPay/Views/PaymentDotPay/PaymentInfo.cshtml");
        }
    }
}
