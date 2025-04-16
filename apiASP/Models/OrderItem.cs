using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; }
        public Service Service { get; set; }
    }
} 