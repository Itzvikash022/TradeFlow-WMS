using System.ComponentModel.DataAnnotations.Schema;
using WMS_Application.Models;

namespace WMS_Application.DTO
{
    public class ProductReportsDTO
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public int? CompanyId { get; set; }

        public DateTime? BoughtOn { get; set; }

        public string? Manufacturer { get; set; }

        public string? ProductImagePath { get; set; }

        public int UnregCompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UnregCompanyName { get; set; }
        public int ProductQty { get; set; }
        public int ShopPrice { get; set; }
        public int PricePerUnit { get; set; }

        public int SalesCount { get; set; }
    }

}
