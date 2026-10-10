using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class DischargeConfiguration : IEntityTypeConfiguration<Discharge>
{
    public void Configure(EntityTypeBuilder<Discharge> builder)
    {
        builder.ToTable("Discharges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.HasOne(x => x.Admission).WithMany(x => x.Discharges)
            .HasForeignKey(x => x.AdmissionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
