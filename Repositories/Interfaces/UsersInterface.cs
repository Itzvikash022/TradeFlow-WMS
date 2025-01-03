using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface UsersInterface
    {
        Task<object> SaveUsers(User user);
        Task<bool> IsUsernameExists(string Username);
        Task<bool> IsEmailExists(string Email);
        Task<bool> IsAdminExists(string AdminRef);
        Task<List<Designation>> GetDesignations();
    }

}

