using WMS_Application.DTO;
using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        decimal GetTotalSalesThisMonth(int shopId);
        decimal GetTotalPurchaseThisMonth(int shopId);
        int GetTotalOrdersThisMonth(int shopId);
        int GetOverallPendingOrders(int shopId);
        int GetBuyers(int shopId);
        int GetSellers(int shopId);
        int GetTotalProductQty(int shopId);
        int GetTotalProductQtyCompany(int companyId);
        int GetUniqueProductQty(int shopId);
        int GetUniqueProductQtyCompany(int companyId);
        List<MonthlySalesPurchaseDto> GetMonthlySalesPurchaseData(int year, int shopId);
        List<TblProduct> RecentlyAddedProductsShop(int shopId);
        List<TblProduct> RecentlyAddedProductsCompany(int companyId);
        List<TblProduct> GetLowStockProductsForShop(int shopId);
        List<TblProduct> GetLowStockProductsForCompany(int companyId);
        int GetEmployeeCountByShop(int adminId);
        int GetProfitThisMonth(int shopId);
    }
}
