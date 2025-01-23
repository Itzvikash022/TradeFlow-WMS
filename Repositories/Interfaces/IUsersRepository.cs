using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<object> SaveUsers(TblUser user);
        Task<bool> IsUsernameExists(string Username);
        Task<bool> IsEmailExists(string Email);
        //Task<bool> IsAdminExists(string AdminRef);
        //Task<List<Designation>> GetDesignations();
        //Task<List<string>> GetAdminUsernames();
        Task<bool> OtpVerification(string Otp);
        Task<object> updateStatus(string Email);
        Task<object> SaveMoreDetails(TblUser user);
        Task<object> SaveShopDetails(TblShop shop, int id);
        Task<object> SaveAdminDoc(TblAdminInfo info, int id);
        Task<bool> hasShopDetails(int id);
        Task<bool> IsVerified(string cred);
        Task<string> fetchEmail(string cred);
        Task<bool> hasAdminDoc(int id);
        Task<TblUser> GetUserDataByEmail(string email);
        Task<TblShop> GetShopDataByUserId(int id);
        Task<TblAdminInfo> GetAdminDocDetailsById(int id);
    }

}

