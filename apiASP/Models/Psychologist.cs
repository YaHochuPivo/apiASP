using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    [Table("Psychologists")]
    public class Psychologist
    {
        [Key]
        public int PsychologistId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(100)]
        public string Specialization { get; set; }

        [Column("Bio")]
        public string Biography { get; set; }

        [Column("ExperienceYears")]
        public int YearsOfExperience { get; set; }

        [StringLength(255)]
        public string PhotoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
} 