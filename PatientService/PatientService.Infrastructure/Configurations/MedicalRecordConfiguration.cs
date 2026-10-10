using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(4000);
        builder.HasOne(x => x.Admission).WithMany(x => x.MedicalRecords)
            .HasForeignKey(x => x.AdmissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
