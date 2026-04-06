using System.Diagnostics;

namespace Apex.Api.Middleware;

public class PerformanceHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Wait for the rest of the pipeline to finish
        context.Response.OnStarting(() =>
        {
            sw.Stop();
            // Add the custom header to the response
            context.Response.Headers["X-Response-Time-ms"] = $"{sw.Elapsed.TotalMilliseconds:F2}";
            return Task.CompletedTask;
        });

        await next(context);
    }
}