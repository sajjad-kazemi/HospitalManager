namespace PatientService.Application.Patients;

public interface IPatientRegistrationService
{
    Task<PatientDetails> RegisterAsync(RegisterPatientRequest request, CancellationToken cancellationToken);
    Task<PatientDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
