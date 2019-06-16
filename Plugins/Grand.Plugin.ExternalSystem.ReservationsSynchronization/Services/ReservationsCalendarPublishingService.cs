using Ical.Net;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services
{
    public class ReservationsCalendarPublishingService
    {
        public Task<Calendar> GetCalendar(string productId, string resourceId, int daysOffset)
        {
            return Task.FromResult(new Calendar());
        }
    }
}
