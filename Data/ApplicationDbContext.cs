using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Controllers;
using SmartClinicSystem.Models;

namespace SmartClinicSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent multi-cascade-delete path errors in SQL Server
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-one relationship: Appointment <-> Prescription
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Prescription)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Prescription>(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed a couple of doctors so the app is usable immediately
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    FullName = "Dr. Anjali Sharma",
                    Specialization = "General Physician",
                    Phone = "9876543210",
                    Email = "anjali.sharma@smartclinic.com",
                    ExperienceYears = 8,
                    AvailableFrom = new TimeSpan(9, 0, 0),
                    AvailableTo = new TimeSpan(17, 0, 0)
                },
                new Doctor
                {
                    Id = 2,
                    FullName = "Dr. Rohan Mehta",
                    Specialization = "Cardiologist",
                    Phone = "9876500000",
                    Email = "rohan.mehta@smartclinic.com",
                    ExperienceYears = 12,
                    AvailableFrom = new TimeSpan(10, 0, 0),
                    AvailableTo = new TimeSpan(18, 0, 0)
                }
            );
        }
    }
}
