using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.ExternalSystem.ReservationsSynchronization.ReservationsCalendar",
                    "Api/ReservationsCalendar/{id}",
                    new { controller = "ReservationsCalendar", action = "GetReservations" }
            );
        }
    }
}