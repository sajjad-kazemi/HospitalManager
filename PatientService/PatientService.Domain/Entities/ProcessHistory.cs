using PatientService.Domain.Enums;

namespace PatientService.Domain.Entities;

public sealed class ProcessHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ProcessType ProcessType { get; set; }
    // Refers to an admission, transfer, or discharge; it is not a database foreign key.
    public Guid ProcessId { get; set; }
    public string Step { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public Guid PerformedByUserId { get; set; }
    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;
}
