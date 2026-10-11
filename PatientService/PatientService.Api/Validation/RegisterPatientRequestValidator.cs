using FluentValidation;
using PatientService.Application.Patients;

namespace PatientService.Api.Validation;

public sealed class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequest>
{
    public RegisterPatientRequestValidator()
    {
        RuleFor(x => x.NationalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.");
        RuleFor(x => x.Gender).IsInEnum();
    }
}
