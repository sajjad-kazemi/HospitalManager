using Microsoft.EntityFrameworkCore;
using PatientService.Application.Patients;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Patients;

public sealed class PatientRepository(PatientDbContext dbContext) : IPatientRepository
{
    public Task<bool> NationalCodeExistsAsync(string nationalCode, CancellationToken cancellationToken) =>
        dbContext.Patients.AnyAsync(patient => patient.NationalCode == nationalCode, cancellationToken);

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Patients.AsNoTracking()
            .SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);

    public void Add(Patient patient) => dbContext.Patients.Add(patient);
}
