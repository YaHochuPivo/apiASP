using System.ComponentModel.DataAnnotations.Schema;

namespace apiASP.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; } = "Pending";

        public Order Order { get; set; }
    }
} 