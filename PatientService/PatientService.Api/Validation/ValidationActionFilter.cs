using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PatientService.Api.Validation;

public sealed class ValidationActionFilter<TRequest>(IValidator<TRequest> validator)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var request = context.ActionArguments.Values.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            // ApiController's model binding handles missing or malformed request bodies.
            await next();
            return;
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (result.IsValid)
        {
            await next();
            return;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
    }
}
