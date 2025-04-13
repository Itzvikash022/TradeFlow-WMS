using WMS_Application.Models;

namespace WMS_Application.DTO
{
    public class DashboardStatsDTO
    {
        public decimal? TotalSalesThisMonth { get; set; }
        public decimal? TotalPurchaseThisMonth { get; set; }
        public int? TotalOrdersThisMonth { get; set; }
        public int? OverallPendingOrders { get; set; }
        public int? Buyers { get; set; }
        public int? Sellers { get; set; }
        public int? EmployeeCount { get; set; }
        public int? TotalProductQty { get; set; }
        public int? UniqueProductQty { get; set; }
        public List<TblProduct>? RecentlyAddedProducts { get; set; } = new List<TblProduct>();
        public List<TblProduct>? LowStockProducts { get; set; } = new List<TblProduct>();
    }
}
