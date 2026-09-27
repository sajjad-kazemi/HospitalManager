using FluentValidation;
using PatientService.Api.Controllers;

namespace PatientService.Api.Validation;

public sealed class LoginApiRequestValidator : AbstractValidator<LoginApiRequest>
{
    public LoginApiRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password).NotEmpty();
    }
}
