# GymRoute

ASP.NET Core MVC gym management application demonstrating **EF Core**, **DbContext abstractions**, **Dependency Injection**, **SOLID**, **Autofac**, **exception handling**, **background jobs**, and **Serilog + Seq**.

## Requirements checklist

| # | Requirement | Status |
|---|-------------|--------|
| 1 | Clean code | Implemented — layered services, no dead tutorial comments |
| 2 | Meaningful naming | `IGymDbContext`, `GlobalExceptionHandler`, `SoftDeletedRecordsPurgeService`, etc. |
| 3 | Practical implementations in Gym MVC | All features wired in `GymRoute.Presentation` |
| 4 | Comments only when necessary | Non-obvious types only (e.g. adapter, exception handler) |
| 5 | Features committed properly | Feature commits on `feature/gym-Reham` |
| 6 | Pull Request + working build | See [Delivery](#delivery) |

## Delivery

1. **Build:** `dotnet build GymRoute.Presentation`
2. **Run:** `dotnet run --project GymRoute.Presentation`
3. **Seq (optional):** `docker compose -f docker-compose.seq.yml up -d`
4. **Pull Request:** open from `feature/gym-Reham` → `master`

## Projects

| Project | Role |
|---------|------|
| `GymRoute.Presentation` | MVC UI, `Program.cs`, Autofac, DI lifetime demo |
| `GymRoute.BusinessLogic` | Services, `IGymDbContext`, `IPlanService` |
| `GymRoute.DataAccess` | EF Core `GymDbContext`, entities, interceptors, migrations |

## Run

```bash
dotnet run --project GymRoute.Presentation
```

| Demo | URL |
|------|-----|
| Plans | `/Plans` |
| DI lifetimes (Guid per instance) | `/DiLifetime` |
| Trigger 500 error (demo) | `/Home/ThrowTest` |
| Trigger 404 error (demo) | `/Home/ThrowNotFound` |
| Error page (after redirect) | `/Home/Error` |

---

## 2. Repository Pattern & EF Core

### 8. Repository Pattern (research)

The **Repository Pattern** (from Domain-Driven Design) hides data access behind an interface so application code talks to repositories, not SQL or EF directly.

| Concept | Role |
|---------|------|
| **Repository** | Collection-like API per aggregate (`GetById`, `Add`, `List`, …) |
| **Unit of Work** | Tracks changes across repositories and commits one transaction |
| **Goal** | Swap persistence, test without a DB, keep domain free of EF types |

Classic flow:

```
Controller/Service → IPlanRepository → PlanRepository → DbContext → Database
```

### 9. Why teams avoid Repository with EF Core

| Reason | Explanation |
|--------|-------------|
| **Thin wrappers** | `GetAllAsync()` → `db.Plans.ToListAsync()` adds little value |
| **Leaky abstraction** | Real apps need `Include`, `Where`, projections — repos grow huge or callers need `IQueryable` |
| **EF Core is already an abstraction** | `DbSet<T>` + LINQ already hide SQL |
| **Harder testing** | Mocking repositories is often harder than in-memory `DbContext` or Testcontainers |
| **Double Unit of Work** | Repo + `DbContext` both expose `SaveChanges`; easy to save in the wrong layer |
| **Maintenance** | Every new entity → new interface + class + DI registration |

Repositories still make sense when you truly swap storage, enforce strict aggregate boundaries, or your team standardizes on them. With EF Core, many teams use **`DbContext` (or an interface over it)** instead.

### 10. DbContext as Repository + Unit of Work

#### As Repository

Each `DbSet<T>` is a typed collection:

| Repository idea | EF Core equivalent |
|-----------------|-------------------|
| `GetById` | `FindAsync` / `FirstOrDefaultAsync` |
| `GetAll` | `ToListAsync()` |
| `Add` | `Add` / `AddAsync` |
| `Delete` | `Remove` |
| Custom queries | LINQ on `DbSet` |

`GymDbContext` exposes: `Plans`, `Categories`, `Users`, `Sessions`, `MemberShips`, `Bookings`, `HealthRecords`.

#### As Unit of Work

One `DbContext` per scope (typically per HTTP request):

- Tracks all entity changes in one change tracker
- `SaveChangesAsync()` commits in **one transaction**
- `AuditInterceptor` runs on that single save

### 11. Interface for DbContext without repositories

Expose what upper layers need: `DbSet` properties + `SaveChangesAsync`. Services depend on **`IGymDbContext`**, not `GymDbContext` or `IPlanRepository`.

**DI registration** (via Autofac module):

```csharp
builder.RegisterType<GymDbContextAdapter>()
    .As<IGymDbContext>()
    .InstancePerLifetimeScope();
```

**Why `GymDbContextAdapter`?**  
`BusinessLogic` references `DataAccess`. If `GymDbContext` implemented `IGymDbContext` inside `DataAccess`, that project would reference `BusinessLogic` → **circular dependency**. The adapter forwards to `GymDbContext` and keeps layers acyclic.

**Alternative (clean architecture):** put `IGymDbContext` in a Core/Application project; `DataAccess` references it and `GymDbContext : DbContext, IGymDbContext` with no adapter.

### 12. Practical examples in this repo

| Approach | Types | Location |
|----------|-------|----------|
| **DbContext interface** | `IGymDbContext` → `GymDbContextAdapter` → `GymDbContext` | `GymRoute.BusinessLogic` |
| **Service (no repository)** | `IPlanService` → `PlanService` | `GymRoute.BusinessLogic/Services` |
| **Cross-aggregate query** | `MemberService` uses `db.Users.OfType<Member>()` | `GymRoute.BusinessLogic/Services` |

```
Repository:     Controller → IPlanRepository → PlanRepository → GymDbContext
DbContext IF:   Controller → IPlanService    → IGymDbContext  → GymDbContext
```

---

## 3. Dependency Injection

### 13. Dependency Injection in ASP.NET Core

**Dependency Injection (DI)** means classes declare what they need (constructor parameters); the **IoC container** in `Program.cs` / Autofac creates objects and wires dependencies.

| Traditional N-tier | ASP.NET Core DI |
|--------------------|-----------------|
| BLL does `new DataAccessLayer()` | `AddScoped<IPlanService, PlanService>()` or Autofac `RegisterType` |
| You choose lifetime manually | Container applies **Transient / Scoped / Singleton** |
| Hard to test (concrete `new`) | Inject interfaces; swap in tests |

### 14. Service lifetimes

| Lifetime | Created | Disposed | Typical use in GymRoute |
|----------|---------|----------|-------------------------|
| **Transient** | Every resolve | GC | Lightweight, stateless helpers |
| **Scoped** | Once per scope (HTTP request) | End of request | `DbContext`, `IPlanService`, `IGymDbContext` |
| **Singleton** | Once per application | App shutdown | `AuditInterceptor`, `IConfiguration` |

**Rules of thumb**

- **DbContext must be Scoped** — never Singleton (not thread-safe, stale data).
- **Do not inject Scoped into Singleton** (captive dependency).

| Built-in DI | Autofac equivalent |
|-------------|-------------------|
| `AddTransient<T>()` | `.InstancePerDependency()` |
| `AddScoped<T>()` | `.InstancePerLifetimeScope()` |
| `AddSingleton<T>()` | `.SingleInstance()` |

### 15–16. Lifetime demo (Guid proves reuse vs recreation)

Open **`/DiLifetime`** and refresh the page.

Each tracked service gets `Guid InstanceId` at construction. `LifetimeComparisonService` resolves each type **five times** in one request.

| Observation | Transient | Scoped | Singleton |
|-------------|-----------|--------|-----------|
| 5 resolves in **same request** | 5 different GUIDs | 5 same GUID | 5 same GUID |
| **Refresh** page (new request) | New GUIDs | New scoped GUID | **Same** singleton GUID |
| Two scoped ctor parameters | — | Same GUID | — |

**Files:** `GymRoute.Presentation/Diagnostics/DependencyInjection/`

---

## 4. SOLID Principles

### 17. SOLID — research summary

| Letter | Principle | One-line idea |
|--------|-----------|---------------|
| **S** | Single Responsibility | One class → one job |
| **O** | Open/Closed | Extend behavior without editing existing code |
| **L** | Liskov Substitution | Subtypes must honor the base type’s contract |
| **I** | Interface Segregation | Small interfaces; no unused methods |
| **D** | Dependency Inversion | Depend on abstractions, not concrete types |

### 18. Each principle with ASP.NET Core examples

#### S — Single Responsibility Principle

A class should have only **one reason to change**.

| Layer | Responsibility | GymRoute example |
|-------|----------------|------------------|
| Controller | HTTP, routing, status codes | `PlansController` |
| Service | Business rules | `PlanService` |
| Interceptor | Cross-cutting persistence | `AuditInterceptor` |

**Violation:** one controller action that validates, queries EF, sends email, and writes files.

#### O — Open/Closed Principle

Open for **extension**, closed for **modification**.

- Add `AuditInterceptor` instead of editing every `SaveChanges` call.
- Add `IPayment` implementations (`InstaPay`, `Visa`) without changing checkout service.

```csharp
public class CheckoutService(IPayment payment) { ... }
builder.Services.AddScoped<IPayment, InstaPay>();
```

#### L — Liskov Substitution Principle

If `B` inherits `A`, use `B` anywhere you use `A` without surprises.

`Member` and `Trainer` extend `User`. Code on `User` works with `Member`; use `OfType<Member>()` for member-specific behavior.

**Violation:** subclass that throws on operations the base type guarantees (e.g. `Save()` not supported).

#### I — Interface Segregation Principle

Clients should not depend on methods they do not use.

`IPlanService` exposes only plan operations — not bookings, members, newsletters.

`IGymDbContext` is wider (many `DbSet`s); stricter ISP would split into `IPlanStore`, `IBookingStore`, etc.

#### D — Dependency Inversion Principle

High-level modules depend on **abstractions**, not `GymDbContext` or `new Concrete()`.

```
PlansController → IPlanService → IGymDbContext → GymDbContextAdapter → GymDbContext
```

### N-tier vs SOLID + ASP.NET Core

| Traditional N-tier | SOLID + ASP.NET Core |
|--------------------|----------------------|
| UI calls BLL, BLL `new`s DAL | **DIP** — inject `IPlanService`, `IGymDbContext` |
| One big “Manager” class | **SRP** — separate controller, service, interceptor |
| Edit switch per payment type | **OCP** — new `IPayment` implementation |
| Subclass Member/Trainer | **LSP** — honor `User` contract |
| One huge `IDataAccess` | **ISP** — focused interfaces |

---

## 5. Autofac

### 19. Autofac — research summary

**Autofac** is a mature **IoC container** for .NET. In ASP.NET Core you use **`Autofac.Extensions.DependencyInjection`**, which plugs in via `AutofacServiceProviderFactory` while still using `builder.Services` for framework setup (MVC, EF Core).

### 20. Built-in DI vs Autofac

| Topic | Built-in ASP.NET Core DI | Autofac |
|--------|---------------------------|---------|
| **Shipped with** | Framework | NuGet package |
| **Configuration** | `builder.Services.AddScoped<...>()` | `ContainerBuilder` + `Module` |
| **Modules** | No first-class modules | `Module` classes per feature/layer |
| **Advanced** | Keyed services (.NET 8+) | Decorators, assembly scanning, named/keyed, child scopes |
| **Learning curve** | Low | Higher |

Calls to `builder.Services.Add...()` are merged into the Autofac container at startup.

### 21. When teams use Autofac

| Scenario | Why |
|----------|-----|
| Large solutions | `Module` per project keeps registration maintainable |
| Assembly scanning | `RegisterAssemblyTypes` registers many types at once |
| Decorators | Wrap services with logging/caching without changing implementations |
| Named / keyed services | Multiple implementations of `IPayment` |
| Legacy migration | Existing Autofac modules from .NET Framework |

Many new ASP.NET Core apps stay on **built-in DI** unless Autofac features are clearly needed.

### 22. Autofac in GymRoute

**Package:** `Autofac.Extensions.DependencyInjection` 11.0.0

**`Program.cs`:**

```csharp
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<GymAutofacModule>();
});

// Framework stays on builder.Services
builder.Services.AddDbContext<GymDbContext>(...);
builder.Services.AddControllersWithViews();
```

**`GymAutofacModule.cs`** registers:

- `AuditInterceptor` — `SingleInstance`
- `GymDbContextAdapter` → `IGymDbContext` — `InstancePerLifetimeScope`
- Assembly scan: `*Service` in BusinessLogic → `AsImplementedInterfaces()`
- `MemberService`, DI lifetime demo types

| Built-in DI | Autofac |
|-------------|---------|
| `AddSingleton<T>()` | `.SingleInstance()` |
| `AddScoped<T>()` | `.InstancePerLifetimeScope()` |
| `AddTransient<T>()` | `.InstancePerDependency()` |

---

## 6. Exception Handling in ASP.NET Core MVC

### 23–24. Global exception handling with `IExceptionHandler`

ASP.NET Core 8+ supports **`IExceptionHandler`** for centralized exception handling. Register the handler and call **`UseExceptionHandler()`** (no path required).

**`Program.cs`:**

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// ...
app.UseExceptionHandler();
```

**`GlobalExceptionHandler`** (`Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs`):

- Logs the exception with trace ID and path
- Maps `GymNotFoundException` → 404, others → 500
- In Development, caches `ErrorViewModel` details in `IMemoryCache` (survives redirect)
- **Redirects** the browser to `/Home/Error?requestId={traceId}`

### 25. Common `Error.cshtml`

Shared view: **`Views/Shared/Error.cshtml`**  
Model: **`ErrorViewModel`** (status code, title, message, request ID, optional dev details)

### 26–27. Integration and redirect flow

```
User request → exception in controller/service
    → GlobalExceptionHandler.TryHandleAsync()
    → Log + redirect to /Home/Error?requestId=...
    → HomeController.Error() loads model (from cache in Dev)
    → Views/Shared/Error.cshtml
```

**Demo actions** on `HomeController`:

| Action | URL | Result |
|--------|-----|--------|
| `ThrowTest` | `/Home/ThrowTest` | 500 + redirect to error page |
| `ThrowNotFound` | `/Home/ThrowNotFound` | 404 + redirect to error page |
| `Error` | `/Home/Error` | Renders `Error.cshtml` directly |

Any unhandled exception in the app (e.g. in `PlansController`) follows the same path.

---

## 7. Background Jobs

### 28–30. Soft-delete purge job (every 30 days)

Permanently **hard-deletes** all rows where `IsDeleted = true`, using **`BackgroundService`** (built into ASP.NET Core).

**Flow:**

```
SoftDeletedRecordsPurgeBackgroundService (every 30 days)
    → ISoftDeletedRecordsPurgeService.PurgeAsync()
    → GymDbContext.IgnoreQueryFilters() + RemoveRange + SaveChanges
```

**Purge order** (FK-safe): Bookings → MemberShips → Sessions → Users → HealthRecords → Plans → Categories.

**Configuration** (`appsettings.json`):

```json
"SoftDeletePurge": {
  "Enabled": true,
  "IntervalDays": 30,
  "RunOnStartup": false
}
```

Development sets `"RunOnStartup": true` so you can verify the job in logs when the app starts.

**Files:**

| File | Role |
|------|------|
| `BackgroundJobs/SoftDeletedRecordsPurgeBackgroundService.cs` | `BackgroundService` + `PeriodicTimer` |
| `DataAccess/Services/SoftDeletedRecordsPurgeService.cs` | EF purge logic |
| `Program.cs` | `AddHostedService`, `Configure<SoftDeletePurgeOptions>` |

### 32. BackgroundService vs Hangfire vs Quartz.NET

| | **BackgroundService** | **Hangfire** | **Quartz.NET** |
|---|----------------------|--------------|----------------|
| **Shipped with ASP.NET Core** | Yes | No (NuGet + storage) | No (NuGet) |
| **Setup** | Minimal | SQL/Redis storage, dashboard | Scheduler config, jobs |
| **Scheduling** | Manual (`PeriodicTimer`, `Task.Delay`) | Cron expressions, delayed/recurring jobs | Rich cron, calendars, clustering |
| **Persistence** | None (lost if app restarts mid-job) | Jobs stored in DB | Can use ADO job store |
| **Dashboard** | No | Yes (web UI) | Third-party / custom |
| **Best for** | Simple periodic tasks in one app instance | Recurring jobs, retries, admin UI | Complex schedules, many triggers |
| **GymRoute choice** | Used for 30-day purge | — | — |

**When to upgrade:** Use **Hangfire** if you need a dashboard, retries, or durable jobs across restarts. Use **Quartz.NET** for advanced cron (e.g. “last Friday of month”) or clustered schedulers.

---

## 8. Logging

### 33. Logging in ASP.NET Core (research)

ASP.NET Core uses **`Microsoft.Extensions.Logging`** (`ILogger<T>`). Log levels: Trace, Debug, Information, Warning, Error, Critical.

| Concept | Description |
|---------|-------------|
| `ILogger<T>` | Category = type name; inject in controllers/services |
| Providers | Console, Debug, EventSource, **third-party (Serilog)** |
| Structured logging | Named placeholders: `{PlanId}` not string concat |
| `appsettings.json` | `Logging:LogLevel` for built-in provider |

**Serilog** replaces/enhances the default provider with richer sinks (Console, File, **Seq**) and structured properties.

### 34. `ILogger` in GymRoute

| Location | What is logged |
|----------|----------------|
| `PlansController` | Plan list/details requested |
| `PlanService` | Active plans count, create, soft-delete, not found |
| `GlobalExceptionHandler` | Unhandled exceptions (structured) |
| `SoftDeletedRecordsPurgeService` | Business purge operations |
| `SoftDeletedRecordsPurgeBackgroundService` | Job start/finish/failure |

### 35–37. Serilog + Seq

**Packages:** `Serilog.AspNetCore`, `Serilog.Sinks.Seq`

**`Program.cs`:** bootstrap logger, `builder.Host.UseSerilog()`, `Log.CloseAndFlush()` on shutdown.

**`appsettings.json`:**

```json
"Serilog": {
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
  ]
}
```

**Start Seq (Docker):**

```bash
docker compose -f docker-compose.seq.yml up -d
```

Open **http://localhost:5341** to search logs.

### 38. What gets logged

| Category | Mechanism | Example |
|----------|-----------|---------|
| **Requests** | `UseGymRouteRequestLogging()` (Serilog request logging) | `HTTP GET /Plans responded 200 in 45 ms` |
| **Exceptions** | `GlobalExceptionHandler` + `ILogger.LogError` | Status, TraceId, Method, Path, Query |
| **Business operations** | `ILogger` in `PlanService`, purge service | `Plan created. PlanId=1 Name=...` |

Request enrichment: `RequestId`, `RequestHost`, `UserAgent`.

---

## Key files reference

| Topic | Path |
|-------|------|
| Startup + Autofac | `GymRoute.Presentation/Program.cs` |
| Autofac module | `GymRoute.Presentation/DependencyInjection/GymAutofacModule.cs` |
| DbContext | `GymRoute.DataAccess/Data/Contexts/GymDbContext.cs` |
| DbContext abstraction | `GymRoute.BusinessLogic/Interfaces/IGymDbContext.cs` |
| Adapter | `GymRoute.BusinessLogic/GymDbContextAdapter.cs` |
| Plan service | `GymRoute.BusinessLogic/Services/PlanService.cs` |
| Audit interceptor | `GymRoute.DataAccess/Interceptors/AuditInterceptor.cs` |
| DI lifetime demo | `GymRoute.Presentation/Diagnostics/DependencyInjection/` |
| DI lifetime UI | `GymRoute.Presentation/Controllers/DiLifetimeController.cs` |
| Global exception handler | `GymRoute.Presentation/Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs` |
| Error view | `GymRoute.Presentation/Views/Shared/Error.cshtml` |
| Soft-delete purge job | `GymRoute.Presentation/BackgroundJobs/SoftDeletedRecordsPurgeBackgroundService.cs` |
| Purge service | `GymRoute.DataAccess/Services/SoftDeletedRecordsPurgeService.cs` |
| Serilog request logging | `GymRoute.Presentation/Infrastructure/Logging/SerilogRequestLoggingExtensions.cs` |
| Seq Docker compose | `docker-compose.seq.yml` |
