using Autofac;
using Autofac.Extensions.DependencyInjection;
using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Data.Seeder;
using GymRoute.DataAccess.Interceptors;
using GymRoute.DataAccess.Services;
using GymRoute.Presentation.BackgroundJobs;
using GymRoute.Presentation.DependencyInjection;
using GymRoute.Presentation.Infrastructure.ExceptionHandling;
using GymRoute.Presentation.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "GymRoute"));

    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule<GymAutofacModule>();
    });

    builder.Services.AddDbContext<GymDbContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

        var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
        options.AddInterceptors(auditInterceptor);
    });

    builder.Services.AddMemoryCache();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddControllersWithViews();

    builder.Services.Configure<SoftDeletePurgeOptions>(
        builder.Configuration.GetSection(SoftDeletePurgeOptions.SectionName));
    builder.Services.AddScoped<ISoftDeletedRecordsPurgeService, SoftDeletedRecordsPurgeService>();
    builder.Services.AddHostedService<SoftDeletedRecordsPurgeBackgroundService>();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseGymRouteRequestLogging();

    app.UseRouting();
    app.UseAuthorization();
    app.MapStaticAssets();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
    await DatabaseSeeder.SeedAllAsync(dbContext);

    Log.Information("GymRoute started. Environment={Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "GymRoute terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
