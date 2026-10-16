using System.ComponentModel.DataAnnotations;

namespace SmartClinicSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Range(0, 120, ErrorMessage = "Enter a valid age")]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(500)]
        [Display(Name = "Medical History")]
        public string? MedicalHistory { get; set; }

        [Display(Name = "Registered On")]
        public DateTime RegisteredOn { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
