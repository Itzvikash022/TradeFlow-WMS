using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WMS_Application.DTO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly dbMain _context;
        public AdminDashboardRepository(dbMain context)
        {
            _context = context;
        }

        public int GetActiveAdminsCount()
        {
            int adminCount = _context.TblUsers
                .Where(x => x.RoleId == 2 && x.IsActive == true && x.IsDeleted == false)
                .Count();
            return adminCount;
        }

        public int GetTotalCompaniesCount()
        {
            int companyCount = _context.TblCompanies
                .Where(x => x.IsActive == true && x.IsDeleted == false)
                .Count();
            return companyCount;
        }

        public int GetSuccessfulOrdersThisMonth()
        {
            int successfulOrders = _context.TblOrders
                .Where(x => x.OrderStatus == "Success" && x.OrderDate.Value.Month == DateTime.Now.Month && x.OrderDate.Value.Year == DateTime.Now.Year)
                .Count();
            return successfulOrders;
        }

        public int GetTotalRegisteredUsers()
        {
            int admin = _context.TblUsers
                .Where(x => x.IsDeleted == false && x.RoleId == 2)
                .Count();
            int company = _context.TblCompanies
                .Where(x => x.IsDeleted == false)
                .Count();
            int totalUsers = admin + company;
            return totalUsers;
        }

        public async Task<List<MonthlyUserJoiningsDTO>> GetUserStatsByYearAsync(int year)
        {
            var users = await _context.TblUsers
            .Where(u => u.RoleId == 2 &&
                        u.CreatedAt.HasValue &&
                        u.CreatedAt.Value.Year == year &&
                        u.IsDeleted != true)
            .ToListAsync();


            var companies = await _context.TblCompanies
              .Where(c => c.CreatedAt.HasValue &&
                          c.CreatedAt.Value.Year == year &&
                          c.IsDeleted != true)
              .ToListAsync();


            var monthlyStats = Enumerable.Range(1, DateTime.Now.Year == year ? DateTime.Now.Month : 12)
             .Select(month => new MonthlyUserJoiningsDTO
             {
                 MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                 RegisteredCount =
                     users.Count(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Month == month) +
                     companies.Count(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Month == month),

                 ActiveCount =
                     users.Count(u =>
                         u.CreatedAt.HasValue &&
                         u.CreatedAt.Value.Month == month &&
                         u.IsActive == true &&
                         u.IsVerified == true &&
                         u.VerificationStatus == "Approved") +
                     companies.Count(c =>
                         c.CreatedAt.HasValue &&
                         c.CreatedAt.Value.Month == month &&
                         c.IsActive == true)
             })
             .ToList();

            return monthlyStats;
        }

        public List<LatestUserDTO> GetRecentlyAddedUsers()
        {
            var shopUsers = _context.TblUsers
                .Where(u => u.RoleId == 2 && u.IsDeleted != true)
                .Select(u => new LatestUserDTO
                {
                    ImagePath = u.ProfileImgPath,
                    FullName = u.Username,
                    UserType = "ShopOwner",
                    Status = u.VerificationStatus,
                    RegisteredOn = u.CreatedAt
                }).ToList();

            var companies = _context.TblCompanies
                .Where(c => c.IsDeleted != true)
                .Select(c => new LatestUserDTO
                {
                    ImagePath = c.CompanyLogo,
                    FullName = c.CompanyName,
                    UserType = "Company",
                    Status = "Approved",
                    RegisteredOn = c.CreatedAt
                }).ToList();

            var recentUsers = shopUsers
                .Concat(companies)
                .OrderByDescending(u => u.RegisteredOn)
                .Take(8)
                .ToList();

            return recentUsers;
        }

    }

}
