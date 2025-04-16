using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
} 