using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Converters;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Tasks;
using Grand.Services.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            TypeDescriptor.AddAttributes(typeof(List<ReservationsSynchronizationSettings.ExternalCalendarSettings>), new TypeConverterAttribute(typeof(CollectionToJsonTypeConverter<ReservationsSynchronizationSettings.ExternalCalendarSettings>)));
            TypeDescriptor.AddAttributes(typeof(List<ReservationsSynchronizationSettings.PublishedCalendarSettings>), new TypeConverterAttribute(typeof(CollectionToJsonTypeConverter<ReservationsSynchronizationSettings.PublishedCalendarSettings>)));

            builder.RegisterType<ReservationsSynchronizationPlugin>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsCalendarPublishingService>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsSynchronizationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReservationsSynchronizationTask>().As<IScheduleTask>().InstancePerLifetimeScope();
        }
    }
}
