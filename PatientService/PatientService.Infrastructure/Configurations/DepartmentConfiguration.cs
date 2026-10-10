using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            new Department
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Emergency",
                Code = "EMERGENCY"
            },
            new Department
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "ICU",
                Code = "ICU"
            },
            new Department
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "Cardiology",
                Code = "CARDIOLOGY"
            },
            new Department
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "General",
                Code = "GENERAL"
            });
    }
}
