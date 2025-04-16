using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    [Table("Services")]
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int DurationMinutes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int? PsychologistId { get; set; }

        [ForeignKey("PsychologistId")]
        public virtual Psychologist Psychologist { get; set; }

        // Навигационные свойства
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
} 