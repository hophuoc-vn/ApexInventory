using Microsoft.AspNetCore.Mvc;
using System.Net;
using Apex.Domain.Exceptions;

namespace Apex.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title) = exception switch
        {
            // 1. Resource missing in the 100k seed
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),

            // 2. Business rule violation (e.g., Stock < 0)
            InvalidOperationException => (HttpStatusCode.BadRequest, "Business Rule Violation"),

            // 3. Specific Concurrency Exception (The Shield)
            ConcurrencyException => (HttpStatusCode.Conflict, "Data Conflict"),

            // 4. Any other exception that mentions "Concurrency" in the message
            Exception e when e.Message.Contains("Concurrency") => (HttpStatusCode.Conflict, "Data Conflict"),

            // 5. Everything else
            _ => (HttpStatusCode.InternalServerError, "Server Error")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (env.IsDevelopment())
        {
            response.Extensions.Add("stackTrace", exception.StackTrace);
        }

        await context.Response.WriteAsJsonAsync(response);
    }
}