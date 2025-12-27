using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DailyCoder.Api.Middlewares;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
    {
        return exception switch
        {
            BadHttpRequestException badRequestEx =>
                WriteProblemDetails(
                    httpContext,
                    badRequestEx,
                    "Bad Request",
                    StatusCodes.Status400BadRequest),

            NotSupportedException notSupportedEx =>
                WriteProblemDetails(
                    httpContext,
                    notSupportedEx,
                    "Unsupported Media Type",
                    StatusCodes.Status415UnsupportedMediaType),

            FluentValidation.ValidationException validationEx =>
                     WriteValidationProblemDetails(httpContext, validationEx),

            _ =>
                WriteProblemDetails(
                    httpContext,
                    exception,
                    "An unexpected error occurred.",
                    StatusCodes.Status500InternalServerError)
        };
    }

    private ValueTask<bool> WriteProblemDetails(
    HttpContext httpContext,
    Exception exception,
    string title,
    int statusCode)
    {
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = exception.Message
            }
        });
    }

    private ValueTask<bool> WriteValidationProblemDetails(
        HttpContext httpContext,
        FluentValidation.ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = new ProblemDetails
        {
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred."
        };

        problemDetails.Extensions["errors"] = errors;

        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }


}
