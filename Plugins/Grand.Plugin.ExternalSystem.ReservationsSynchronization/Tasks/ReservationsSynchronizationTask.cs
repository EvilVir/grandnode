using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services;
using Grand.Services.Tasks;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Tasks
{
    public class ReservationsSynchronizationTask : IScheduleTask
    {
        protected readonly ReservationsSynchronizationService _synchronizationService;

        public ReservationsSynchronizationTask(ReservationsSynchronizationService synchronizationService)
        {
            this._synchronizationService = synchronizationService;
        }

        public async Task Execute()
        {
            await _synchronizationService.SynchronizeExternalCalendars();
        }
    }
}
