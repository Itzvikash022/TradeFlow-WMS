using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        Task<List<TblOrder>> GetAllOrders();
    }
}
