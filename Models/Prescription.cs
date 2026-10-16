using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartClinicSystem.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

        [Required(ErrorMessage = "Please enter a diagnosis")]
        [StringLength(300)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please list the prescribed medicines")]
        [StringLength(500)]
        [Display(Name = "Prescribed Medicines")]
        public string Medicines { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Follow-up Date")]
        [DataType(DataType.Date)]
        public DateTime? FollowUpDate { get; set; }

        [Display(Name = "Issued On")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}