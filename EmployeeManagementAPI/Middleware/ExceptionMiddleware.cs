using Microsoft.Extensions.Logging;
namespace EmployeeManagementAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

    context.Response.ContentType = "application/json";
    _logger.LogError(ex, "Unhandled exception occurred.");
    var statusCode = ex switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    };

    context.Response.StatusCode = statusCode;

    await context.Response.WriteAsJsonAsync(new
    {
        StatusCode = statusCode,
        Message = ex.Message,
        Timestamp = DateTime.UtcNow
    });
}
        }
    }

