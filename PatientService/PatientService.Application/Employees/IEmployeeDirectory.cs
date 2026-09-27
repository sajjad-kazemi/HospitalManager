namespace PatientService.Application.Employees;

public interface IEmployeeDirectory
{
    Task<EmployeeDetails?> GetByIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken);
}
