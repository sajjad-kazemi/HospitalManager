using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.HasOne(x => x.Admission).WithMany(x => x.Transfers)
            .HasForeignKey(x => x.AdmissionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FromDepartment).WithMany(x => x.OutgoingTransfers)
            .HasForeignKey(x => x.FromDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToDepartment).WithMany(x => x.IncomingTransfers)
            .HasForeignKey(x => x.ToDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
