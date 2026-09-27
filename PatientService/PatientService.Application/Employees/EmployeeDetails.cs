namespace PatientService.Application.Employees;

public sealed record EmployeeDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt);
