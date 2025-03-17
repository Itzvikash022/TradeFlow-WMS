using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.DTO
{
    public class TransactionReportsDTO
    {
        public int TransactionId { get; set; }

        public int? OrderId { get; set; }

        public string TransactionType { get; set; } = null!;
        public string ReceiptPath { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? OrderDate { get; set; }

    }

}
