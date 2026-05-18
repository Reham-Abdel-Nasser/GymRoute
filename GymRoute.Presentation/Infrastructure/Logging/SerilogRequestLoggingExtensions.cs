using Serilog;
using Serilog.Events;

namespace GymRoute.Presentation.Infrastructure.Logging;

public static class SerilogRequestLoggingExtensions
{
    public static WebApplication UseGymRouteRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            };

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex is not null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode > 499)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode > 399)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });

        return app;
    }
}
