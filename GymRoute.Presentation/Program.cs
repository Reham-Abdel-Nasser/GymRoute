using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Data.Seeder;
using GymRoute.DataAccess.Interceptors;
using GymRoute.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conventional Middleware => lifetime => Singleton
// IOC Container: Send objects to DI
// Inversion of control Container

builder.Services.AddScoped<IPlanRepository, PlanRepository>();

builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<GymDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    options.AddInterceptors(auditInterceptor);
});

// builder.Services.AddScoped<IPayment, InstaPay>();
// builder.Services.AddScoped<IPayment, Visa>(); // the last is executed

// if use multiple type from any services 
// Feature => Keyservices

builder.Services.AddKeyedScoped<IPlanRepository, PlanRepository>("Plan");
builder.Services.AddKeyedScoped<IPlanRepository, PlanRepository>("sfsf");

// Scoped : For each Request => Only one instance
// after the request ends => database

// Transient => request => 100 => 100 instances
// after the request ends =>

// Singleton : object for all requests

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
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

app.Run();
