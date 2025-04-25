using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.DTO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly dbMain _context;
        private readonly IPermisionHelperRepository _permission;
        private readonly IDashboardRepository _dashboard;
        private readonly IAdminDashboardRepository _adminDashboard;

        public DashboardController(dbMain context, ISidebarRepository sidebar, IPermisionHelperRepository permision, IDashboardRepository dashboard, IAdminDashboardRepository adminDashboard) : base(sidebar)
        {
            _context = context;
            _permission = permision;
            _dashboard = dashboard;
            _adminDashboard = adminDashboard;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;

            string permissionType = _permission.HasAccess("Dashboard", roleId);
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int shopId = 0;
                if(roleId != 5)
                {
                    shopId = (int) HttpContext.Session.GetInt32("ShopId");
                }
                else
                {
                    shopId = (int)HttpContext.Session.GetInt32("CompanyId");
                }

                int productQty, uniqueQty, EmployeeCount, ProfitCount;
                List<TblProduct> recentlyAddedProducts = new List<TblProduct>();
                if (roleId == 5)
                {
                    productQty = _dashboard.GetTotalProductQtyCompany(shopId);
                    uniqueQty = _dashboard.GetUniqueProductQtyCompany(shopId);
                    recentlyAddedProducts = _dashboard.RecentlyAddedProductsCompany(shopId);
                    EmployeeCount = 0;
                    ProfitCount = 0;
                }
                else
                {
                    productQty = _dashboard.GetTotalProductQty(shopId);
                    uniqueQty = _dashboard.GetUniqueProductQty(shopId);
                    recentlyAddedProducts = _dashboard.RecentlyAddedProductsShop(shopId);
                    EmployeeCount = _dashboard.GetEmployeeCountByShop((int)HttpContext.Session.GetInt32("UserId"));
                    ProfitCount = _dashboard.GetProfitThisMonth(shopId);
                }
                DashboardStatsDTO dashboardData = new DashboardStatsDTO
                {
                    TotalSalesThisMonth = _dashboard.GetTotalSalesThisMonth(shopId),
                    TotalPurchaseThisMonth = _dashboard.GetTotalPurchaseThisMonth(shopId),
                    TotalOrdersThisMonth = _dashboard.GetTotalOrdersThisMonth(shopId),
                    OverallPendingOrders = _dashboard.GetOverallPendingOrders(shopId),
                    Buyers = _dashboard.GetBuyers(shopId),
                    Sellers = _dashboard.GetSellers(shopId),
                    TotalProductQty = productQty,
                    UniqueProductQty = uniqueQty,
                    RecentlyAddedProducts = recentlyAddedProducts,
                    LowStockProducts = _dashboard.GetLowStockProductsForShop(shopId),
                    EmployeeCount = EmployeeCount,
                    ProfitThisMonth = ProfitCount
                };

                return View(dashboardData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [Route("AdminDashboard")]
        public IActionResult AdminDashboard()
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess("AdminDashboard", roleId);
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                SuperAdminDashboardDto superAdminDashboard = new SuperAdminDashboardDto
                {
                    ActiveAdminsCount = _adminDashboard.GetActiveAdminsCount(),
                    TotalCompaniesCount = _adminDashboard.GetTotalCompaniesCount(),
                    SuccessfulOrdersThisMonth = _adminDashboard.GetSuccessfulOrdersThisMonth(),
                    TotalRegisteredUsers = _adminDashboard.GetTotalRegisteredUsers(),
                    LatestRegisteredUsers = _adminDashboard.GetRecentlyAddedUsers()
                };

                return View(superAdminDashboard);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet("user-activity-stats/{year}")]
        public async Task<IActionResult> GetUserActivityStats(int year)
        {
            var data = await _adminDashboard.GetUserStatsByYearAsync(year);
            return Ok(data);
        }


        [Route("MyProfile")]
        public async Task<IActionResult> MyProfile()
        {
            var permissionType = GetUserPermission("My Profile");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int UserId = (int)HttpContext.Session.GetInt32("UserId");
                TblUser userData = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == UserId);
                return View(userData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public IActionResult GetSalesPurchaseData(int year)
        {
            int id = 0;
            if((int)HttpContext.Session.GetInt32("UserRoleId") == 5)
            {
                id = (int)HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                id = (int)HttpContext.Session.GetInt32("ShopId");
            }
            var data = _dashboard.GetMonthlySalesPurchaseData(year, id);
            return Json(data);
        }



    }
}
