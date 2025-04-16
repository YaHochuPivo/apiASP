using System.ComponentModel.DataAnnotations;

namespace apiASP.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int PsychologistId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public Psychologist Psychologist { get; set; }
    }
} 