using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.Payments.DotPay
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //URLC
            routeBuilder.MapRoute("Plugin.Payments.DotPay.CallbackHandler",
                 "Plugins/PaymentDotPay/Callback",
                 new { controller = "PaymentDotPay", action = "HandleCallback" }
            );
        }
    }
}
