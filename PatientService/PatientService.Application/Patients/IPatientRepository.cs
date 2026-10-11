using PatientService.Domain.Entities;

namespace PatientService.Application.Patients;

public interface IPatientRepository
{
    Task<bool> NationalCodeExistsAsync(string nationalCode, CancellationToken cancellationToken);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Patient patient);
}
