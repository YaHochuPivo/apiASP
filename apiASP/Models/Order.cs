using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";

        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
} 