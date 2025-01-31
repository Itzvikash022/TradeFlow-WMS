using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<List<TblRole>> GetAllRoles();
    }
}
