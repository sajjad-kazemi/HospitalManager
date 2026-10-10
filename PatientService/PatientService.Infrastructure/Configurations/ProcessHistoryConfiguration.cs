using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class ProcessHistoryConfiguration : IEntityTypeConfiguration<ProcessHistory>
{
    public void Configure(EntityTypeBuilder<ProcessHistory> builder)
    {
        builder.ToTable("ProcessHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Step).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FromStatus).HasMaxLength(30);
        builder.Property(x => x.ToStatus).IsRequired().HasMaxLength(30);
        // ProcessId and employee IDs are identifiers, not cross-service foreign keys.
    }
}
