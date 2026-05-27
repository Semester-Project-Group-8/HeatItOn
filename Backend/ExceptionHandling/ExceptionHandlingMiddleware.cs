using System.Net;
using System.Security.Authentication;
using Backend.ExceptionHandling.Exceptions;
using Microsoft.EntityFrameworkCore;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Backend.ExceptionHandling;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await  _next(context);
        } catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;

        Type? exceptionType = exception.GetType();

        string message = exception.Message;
        if (exceptionType == typeof(DirectoryNotFoundException) ||
            exceptionType == typeof(DllNotFoundException) ||
            exceptionType == typeof(EntryPointNotFoundException) ||
            exceptionType == typeof(FileNotFoundException) ||
            exceptionType == typeof(KeyNotFoundException))
        {
            status = HttpStatusCode.NotFound;
        }
        else if (exceptionType == typeof(NotImplementedException))
        {
            status = HttpStatusCode.NotImplemented;
        }
        else if (exceptionType == typeof(UnauthorizedAccessException) ||
                 exceptionType == typeof(AuthenticationException))
        {
            status = HttpStatusCode.Unauthorized;
        }
        else if (exceptionType == typeof(ValidationException) ||
                 exceptionType == typeof(NotSupportedException))
        {
            status =  HttpStatusCode.BadRequest;
        }
        else if (exceptionType == typeof(AlreadyExistsException) ||
                 exceptionType == typeof(ConflictException) ||
                 exceptionType == typeof(DbUpdateException))
        {
            status = HttpStatusCode.Conflict;
        }
        else if (exceptionType == typeof(NotFoundException))
        {
            status = HttpStatusCode.NotFound;
        }
        else
        {
            status = HttpStatusCode.InternalServerError;
        }

        string? stackTrace = exception.StackTrace;

        string exceptionResult = System.Text.Json.JsonSerializer.Serialize(new { error = message, stackTrace });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        
        return context.Response.WriteAsync(exceptionResult);
    }
}