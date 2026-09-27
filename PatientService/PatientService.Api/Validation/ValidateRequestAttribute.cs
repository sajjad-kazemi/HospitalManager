using Microsoft.AspNetCore.Mvc;

namespace PatientService.Api.Validation;

// TypeFilter creates the filter through DI, allowing it to use the request's validator.
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateRequestAttribute(Type requestType)
    : TypeFilterAttribute(typeof(ValidationActionFilter<>).MakeGenericType(requestType));
