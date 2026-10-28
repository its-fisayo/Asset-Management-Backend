using AssetManagementBackend.Exceptions;
using System.Net;
using System.Text.Json;

namespace AssetManagementBackend.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch(Exception exception)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occured"
            );

            await HandleExceptionAsync(
                context,
                exception
            );
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception
    )
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message = exception switch
            {
                BadRequestException => exception.Message,
                NotFoundException => exception.Message,
                ConflictException => exception.Message,
                _ => "An unexpected error occured."
            }
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}