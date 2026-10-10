using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.ToTable("Admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoomNumber).HasMaxLength(20);
        builder.Property(x => x.BedNumber).HasMaxLength(20);
        builder.HasOne(x => x.Patient).WithMany(x => x.Admissions)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany(x => x.Admissions)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PatientId).IsUnique()
            .HasFilter("[Status] IN (0, 1)");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
