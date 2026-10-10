namespace PatientService.Application.Employees;

public interface IEmployeeDirectory
{
    Task<EmployeeDetails?> GetByIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<EmployeeDetails> CreateAsync(
    CreateEmployeeRequest request,
    string authorizationHeader,
    CancellationToken cancellationToken);

    Task<EmployeeDetails> AssignRoleAsync(
    Guid employeeId,
    AssignEmployeeRoleRequest request,
    string authorizationHeader,
    CancellationToken cancellationToken);
}
