namespace WMS_Application.DTO
{
    public class ProductS2SBuyDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal PricePerUnit { get; set; }
        public string CompanyName { get; set; }
        public int ProductQty { get; set; }
        public string ProductImagePath { get; set; }
        public int? SellerShopId { get; set; }
        public int? CompanyId { get; set; }
    }

}
