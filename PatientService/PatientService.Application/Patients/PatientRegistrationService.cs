using PatientService.Application.Errors;
using PatientService.Application.Persistence;
using PatientService.Domain.Entities;

namespace PatientService.Application.Patients;

public sealed class PatientRegistrationService(
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork) : IPatientRegistrationService
{
    public async Task<PatientDetails> RegisterAsync(
        RegisterPatientRequest request, CancellationToken cancellationToken)
    {
        var nationalCode = request.NationalCode.Trim();
        if (await patientRepository.NationalCodeExistsAsync(nationalCode, cancellationToken))
            throw new ConflictException("A patient with this national code already exists.");

        var patient = new Patient
        {
            NationalCode = nationalCode,
            BirthDate = request.BirthDate,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender
        };

        patientRepository.Add(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(patient);
    }

    public async Task<PatientDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(id, cancellationToken);
        return patient is null ? null : Map(patient);
    }

    private static PatientDetails Map(Patient patient) =>
        new(patient.Id, patient.FirstName, patient.LastName, patient.NationalCode,
            patient.BirthDate, patient.Gender, patient.CreatedAt);
}
