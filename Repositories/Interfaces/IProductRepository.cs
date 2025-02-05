using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<TblProduct>> GetAllProducts(int companyId, int shopId);
    }
}
