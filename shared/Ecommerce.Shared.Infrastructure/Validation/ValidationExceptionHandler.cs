using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Shared.Infrastructure.Validation;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var problemDetails = new ValidationProblemDetails(validationException.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed"
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
