using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface UsersInterface
    {
            Task SaveUsers(User user);
            Task<bool> IsUsernameExists(string Username);
            Task<bool> IsEmailExists(string Email);
    }

}

