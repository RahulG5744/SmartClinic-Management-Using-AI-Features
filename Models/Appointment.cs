using SmartClinicSystem.Controllers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartClinicSystem.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(10, 0, 0);

        [StringLength(300)]
        [Display(Name = "Reason for Visit")]
        public string? Reason { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // One-to-one: an appointment can have exactly one prescription once completed
        public Prescription? Prescription { get; set; }
    }
}
