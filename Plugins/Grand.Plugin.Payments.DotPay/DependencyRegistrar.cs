using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Payments.DotPay.Services;

namespace Grand.Plugin.Payments.DotPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<DotPayPaymentProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<DotPayApiService>().InstancePerLifetimeScope();
        }
    }
}
