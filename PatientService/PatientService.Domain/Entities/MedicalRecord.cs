namespace PatientService.Domain.Entities;

public sealed class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdmissionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid DoctorUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Admission Admission { get; set; } = null!;
}
