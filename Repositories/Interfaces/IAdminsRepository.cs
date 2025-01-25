using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IAdminsRepository
    {
        Task<List<TblUser>> GetAllAdminsData();
        Task<object> UpdateStatus(int adminId, bool status);
    }
}
