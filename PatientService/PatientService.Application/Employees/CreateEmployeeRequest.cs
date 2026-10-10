namespace PatientService.Application.Employees;

public sealed record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IReadOnlyList<string> Roles);