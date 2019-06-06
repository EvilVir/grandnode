using Grand.Plugin.ExternalSystem.BookingCom.Services;
using Grand.Services.Tasks;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.BookingCom.Tasks
{
    public class ReservationsFetchingTask : IScheduleTask
    {
        protected readonly BookingComApiService _apiService;

        public ReservationsFetchingTask(BookingComApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task Execute()
        {
            await _apiService.FetchReservations();
        }
    }
}
