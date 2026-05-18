using Autofac;
using GymRoute.BusinessLogic;
using GymRoute.BusinessLogic.Interfaces;
using GymRoute.BusinessLogic.Services;
using GymRoute.DataAccess.Interceptors;
using GymRoute.Presentation.Diagnostics.DependencyInjection;

namespace GymRoute.Presentation.DependencyInjection;

public sealed class GymAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuditInterceptor>()
            .SingleInstance();

        builder.RegisterType<GymDbContextAdapter>()
            .As<IGymDbContext>()
            .InstancePerLifetimeScope();

        // Scan BusinessLogic for *Service types → register as implemented interfaces
        builder.RegisterAssemblyTypes(typeof(PlanService).Assembly)
            .Where(t => t.IsClass && t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // No interface — explicit registration
        builder.RegisterType<MemberService>()
            .InstancePerLifetimeScope();

        // DI lifetime demo
        builder.RegisterType<TransientTrackedService>()
            .As<ITransientTrackedService>()
            .InstancePerDependency();

        builder.RegisterType<ScopedTrackedService>()
            .As<IScopedTrackedService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SingletonTrackedService>()
            .As<ISingletonTrackedService>()
            .SingleInstance();

        builder.RegisterType<LifetimeComparisonService>()
            .InstancePerLifetimeScope();
    }
}
