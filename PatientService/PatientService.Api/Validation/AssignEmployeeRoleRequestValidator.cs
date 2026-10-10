using FluentValidation;
using PatientService.Application.Employees;

namespace PatientService.Api.Validation;

public sealed class AssignEmployeeRoleRequestValidator : AbstractValidator<AssignEmployeeRoleRequest>
{
    public AssignEmployeeRoleRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty().Must(EmployeeRoleValidation.IsKnownRole)
            .WithMessage("Role must be Admin, Receptionist, Doctor, or Nurse.");
    }
}

internal static class EmployeeRoleValidation
{
    public static bool IsKnownRole(string role) => role is "Admin" or "Receptionist" or "Doctor" or "Nurse";
}
