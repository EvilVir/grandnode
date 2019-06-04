using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.ExternalSystem.California
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.ExternalSystem.California.Reservations",
                 "Api/California/Reservations",
                 new { controller = "Reservations", action = "GetReservations" }
            );

            // Backward compatibility with old PHP site
            routeBuilder.MapRoute("Plugin.ExternalSystem.California.BackwardCompatibility.Reservations",
                 "Api/Reservations.php",
                 new { controller = "Reservations", action = "BC_GetReservations" }
            );
        }
    }
}
