using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IShopRepository
    {
        Task<List<TblShop>> GetAllShops();
    }
}
