using Microsoft.EntityFrameworkCore;
using PatientService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatientService.Infrastructure
{
    public sealed class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Discharge> Discharges { get; set; }
        public DbSet<ProcessHistory> ProcessHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientDbContext).Assembly);
        }
    }
}
