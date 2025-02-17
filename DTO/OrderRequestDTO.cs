namespace WMS_Application.DTO
{
    public class OrderRequestDto
    {
        public int CompanyId { get; set; }
        public int CustomerId { get; set; }
        public int ShopId { get; set; }
        public int SellerShopId { get; set; }
        public int TotalQty { get; set; }  // Now passed from frontend
        public decimal TotalAmount { get; set; }  // Now passed from frontend
        public List<ProductDto> Products { get; set; }
    }

}
