using FluentValidation;
using PatientService.Application.Employees;

namespace PatientService.Api.Validation;

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Roles).NotNull();
        RuleForEach(x => x.Roles).NotEmpty().Must(EmployeeRoleValidation.IsKnownRole)
            .WithMessage("Role must be Admin, Receptionist, Doctor, or Nurse.");
        RuleFor(x => x.Roles).Must(roles => roles is null || roles.Distinct(StringComparer.OrdinalIgnoreCase).Count() == roles.Count)
            .WithMessage("Roles must not be repeated.");
    }
}
