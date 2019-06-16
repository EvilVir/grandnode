using Grand.Services.Tasks;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Tasks
{
    public class ReservationsSynchronizationTask : IScheduleTask
    {
        public Task Execute()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
