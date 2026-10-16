using System.ComponentModel.DataAnnotations;

namespace SmartClinicSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Doctor name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Range(0, 60)]
        [Display(Name = "Experience (Years)")]
        public int ExperienceYears { get; set; }

        [Display(Name = "Available From")]
        [DataType(DataType.Time)]
        public TimeSpan AvailableFrom { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Available To")]
        [DataType(DataType.Time)]
        public TimeSpan AvailableTo { get; set; } = new TimeSpan(17, 0, 0);

        // Navigation property
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
