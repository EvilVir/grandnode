using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.ExternalSystem.BookingCom.Services;
using Grand.Plugin.ExternalSystem.BookingCom.Tasks;
using Grand.Services.Tasks;

namespace Grand.Plugin.ExternalSystem.BookingCom
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<BookingComApiPlugin>().InstancePerLifetimeScope();
            builder.RegisterType<BookingComApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsFetchingTask>().As<IScheduleTask>().InstancePerLifetimeScope();
        }
    }
}
