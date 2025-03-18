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
        private readonly IOrdersRepository _orders;
        private readonly IProductRepository _product;
        public ReportsController(ISidebarRepository sidebar, dbMain context, IAdminsRepository admin, IShopRepository shop, ICompanyRepository company, IEmployeeRepository employee, IOrdersRepository orders, IProductRepository product) : base(sidebar)
        {
            _context = context;
            _admin = admin;
            _shop = shop;
            _company = company;
            _employee = employee;
            _orders = orders;
            _product = product;
        }
        public IActionResult AdminReports()
        {
            return View(_admin.GetAdminReports());
        }

        public IActionResult ShopReports()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
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
        public IActionResult TransactionReports()
        {
            int userId = (int)HttpContext.Session.GetInt32("ShopId");
            return View(_orders.GetTransactionReports(userId));
        }
        public IActionResult ProductReports()
        {
            int userId = 0;
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if (roleId == 5)
            {
                userId = (int)HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                userId = (int)HttpContext.Session.GetInt32("ShopId");
            }
            return View(_product.GetProductsReports(userId, roleId));
        }
        public IActionResult ActivityLog()
        {
            return View();
        }
    }
}
