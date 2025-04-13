using WMS_Application.DTO;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IAdminDashboardRepository
    {
        int GetActiveAdminsCount();
        int GetTotalCompaniesCount();
        int GetSuccessfulOrdersThisMonth();
        int GetTotalRegisteredUsers();
        Task<List<MonthlyUserJoiningsDTO>> GetUserStatsByYearAsync(int year);
        List<LatestUserDTO> GetRecentlyAddedUsers();
    }
}
