using WMS_Application.Models;

namespace WMS_Application.DTO
{
    public class OrderDetailsDTO
    {
        // Transaction Details
        public string ReferenceNumber { get; set; }
        public string PaymentType { get; set; }
        public decimal TransactionAmount { get; set; }


        // Order Details
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }

        // Seller & Buyer Details
        public string SellerName { get; set; }
        public string SellerAddress { get; set; }
        public string SellerEmail { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerName { get; set; }
        public string Remarks { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public string BuyerContact { get; set; }

        // Product List
        public List<OrderProductDTO> Products { get; set; }
    }

    public class OrderProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalAmount { get; set; }
    }


}
