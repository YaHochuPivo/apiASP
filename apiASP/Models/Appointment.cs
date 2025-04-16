namespace apiASP.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public int PsychologistId { get; set; }
        public int ServiceId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string Notes { get; set; }
        
        public User User { get; set; }
        public Psychologist Psychologist { get; set; }
        public Service Service { get; set; }
    }
} 