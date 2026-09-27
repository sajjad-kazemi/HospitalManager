using FluentValidation;
using HospitalManager.Contracts.Identity.V1;
using IdentityService.Infrastructure.Identity;

namespace IdentityService.Api.Validation;

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty();

        RuleForEach(request => request.Roles)
            .NotEmpty()
            .Must(role => HospitalRoles.All.Contains(role))
            .WithMessage("Role must be Admin, Receptionist, Doctor, or Nurse.");

        RuleFor(request => request.Roles)
            .Must(roles => roles.Distinct(StringComparer.OrdinalIgnoreCase).Count() == roles.Count)
            .WithMessage("Roles must not be repeated.");
    }
}
