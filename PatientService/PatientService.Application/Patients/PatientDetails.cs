using PatientService.Domain.Enums;

namespace PatientService.Application.Patients;

public sealed record PatientDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string NationalCode,
    DateOnly BirthDate,
    Gender Gender,
    DateTimeOffset CreatedAt);
