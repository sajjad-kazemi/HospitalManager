using PatientService.Domain.Enums;

namespace PatientService.Application.Patients;

public sealed record RegisterPatientRequest(
    string FirstName,
    string LastName,
    string NationalCode,
    DateOnly BirthDate,
    Gender Gender);
