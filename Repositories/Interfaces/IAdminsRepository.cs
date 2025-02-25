using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IAdminsRepository
    {
        Task<List<TblUser>> GetAllAdminsData();
        Task<object> UpdateStatus(int adminId, bool status, string verifier, string remark);
        TblUser checkExistence(string Username, string email, int UserId);
        Task<List<TblRole>> GetAllRoles();
        Task<object> SaveUsers(TblUser user);
    }
}
