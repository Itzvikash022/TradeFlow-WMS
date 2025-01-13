using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface UsersInterface
    {
        Task<object> SaveUsers(User user);
        Task<bool> IsUsernameExists(string Username);
        Task<bool> IsEmailExists(string Email);
        //Task<bool> IsAdminExists(string AdminRef);
        Task<List<Designation>> GetDesignations();
        Task<List<string>> GetAdminUsernames();
        Task<bool> OtpVerification(string Otp);
        Task<object> updateStatus(string Email);
    }

}

