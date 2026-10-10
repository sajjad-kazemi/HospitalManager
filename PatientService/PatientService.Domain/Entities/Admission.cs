using PatientService.Domain.Enums;

namespace PatientService.Domain.Entities;

public sealed class Admission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTimeOffset AdmissionDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DischargeDate { get; set; }
    public string? RoomNumber { get; set; }
    public string? BedNumber { get; set; }
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Draft;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public Patient Patient { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
    public ICollection<Transfer> Transfers { get; set; } = [];
    public ICollection<Discharge> Discharges { get; set; } = [];
}
