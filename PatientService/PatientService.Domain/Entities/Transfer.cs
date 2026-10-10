using PatientService.Domain.Enums;

namespace PatientService.Domain.Entities;

public sealed class Transfer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdmissionId { get; set; }
    public Guid FromDepartmentId { get; set; }
    public Guid ToDepartmentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public TransferStatus Status { get; set; } = TransferStatus.Requested;
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Admission Admission { get; set; } = null!;
    public Department FromDepartment { get; set; } = null!;
    public Department ToDepartment { get; set; } = null!;
}
