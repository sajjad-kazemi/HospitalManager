namespace PatientService.Domain.Entities;

public sealed class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ICollection<Admission> Admissions { get; set; } = [];
    public ICollection<Transfer> OutgoingTransfers { get; set; } = [];
    public ICollection<Transfer> IncomingTransfers { get; set; } = [];
}
