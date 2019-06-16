using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Tasks;
using Grand.Services.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<ReservationsSynchronizationPlugin>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsCalendarPublishingService>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsSynchronizationTask>().As<IScheduleTask>().InstancePerLifetimeScope();
        }
    }
}
