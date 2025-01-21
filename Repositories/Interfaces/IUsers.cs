using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IUsers
    {
        Task<object> SaveUsers(User user);
        Task<bool> IsUsernameExists(string Username);
        Task<bool> IsEmailExists(string Email);
        //Task<bool> IsAdminExists(string AdminRef);
        //Task<List<Designation>> GetDesignations();
        //Task<List<string>> GetAdminUsernames();
        Task<bool> OtpVerification(string Otp);
        Task<object> updateStatus(string Email);
        Task<object> SaveMoreDetails(User user);
        Task<object> SaveShopDetails(Shop shop, int id);
        Task<int?> GetUserIdByEmail(string email);
        Task<object> SaveAdminDoc(AdminInfo info, int id);
    }

}

