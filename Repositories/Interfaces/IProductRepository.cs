using WMS_Application.DTO;
using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<TblProduct>> GetAllProducts(int companyId, int shopId);
        Task<object> SaveShopProductAsync(TblProduct product, int UserId, int ShopId);
        Task SaveStockAsync(int ProductId, int ShopId, int Quantity, int shopPrice);
        Task<int> SaveUnregCompanyAsync(string CompanyName, int AddedBy);
        public List<ProductReportsDTO> GetProductsReports(int userId, int roleId);

    }
}
