using GramQ.Shared.Abstractions.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GramQ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Problem(Error error)
    {
        return error.Type == ErrorType.Validation
            ? ValidationProblem(error)
            : ApplicationProblem(error);
    }

    private IActionResult ApplicationProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var extensionsDictionary = new Dictionary<string, object?>
        {
            { "errorCode", error.Code }
        };

        return Problem(
            statusCode: statusCode,
            title: error.Description,
            extensions: extensionsDictionary);
    }

    private IActionResult ValidationProblem(Error error)
    {
        return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "errors", [error.Code, error.Description] }
        }));
    }
}
