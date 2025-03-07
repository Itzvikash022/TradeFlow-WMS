using Microsoft.AspNetCore.Mvc;
using WMS_Application.DTO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ISidebarRepository sidebar;
        private readonly dbMain _context;
        private readonly IAdminsRepository _admin;
        private readonly IShopRepository _shop;
        private readonly ICompanyRepository _company;
        private readonly IEmployeeRepository _employee;

        public ReportsController(ISidebarRepository sidebar, dbMain context, IAdminsRepository admin, IShopRepository shop, ICompanyRepository company, IEmployeeRepository employee) : base(sidebar)
        {
            _context = context;
            _admin = admin;
            _shop = shop;
            _company = company;
            _employee = employee;
        }
        public IActionResult AdminReports()
        {
            return View(_admin.GetAdminReports());
        }

        public IActionResult ShopReports()
        {
            int userId = (int) HttpContext.Session.GetInt32("UserId"); 
            return View(_shop.GetShopReports(userId));
        }
        public IActionResult CompanyReports()
        {
            return View(_company.GetCompanyReports());
        }
        public IActionResult EmployeeReports()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            return View(_employee.GetEmployeeReports(userId));
        }
        public IActionResult TransacionReports()
        {
            return View();
        }
        public IActionResult ActivityLog()
        {
            return View();
        }
    }
}
