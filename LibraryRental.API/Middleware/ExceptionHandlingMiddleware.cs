using LibraryRental.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRental.API.Middleware;

/// <summary>Turns Application exceptions into consistent ProblemDetails responses.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Server error")
            };

            string detail;
            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception");
                detail = "An unexpected error occurred.";
            }
            else
            {
                detail = ex.Message;
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            });
        }
    }
}
