using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
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
        private readonly IPermisionHelperRepository _permission;

        public ReportsController(ISidebarRepository sidebar, dbMain context, IAdminsRepository admin, IShopRepository shop, ICompanyRepository company, IEmployeeRepository employee, IOrdersRepository orders, IProductRepository product, IPermisionHelperRepository permission) : base(sidebar)
        {
            _context = context;
            _admin = admin;
            _shop = shop;
            _company = company;
            _employee = employee;
            _orders = orders;
            _product = product;
            _permission = permission;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return _permission.HasAccess(action, roleId);
        }

        public IActionResult AdminReports()
        {
            string permissionType = GetUserPermission("Admins Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View(_admin.GetAdminReports());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public IActionResult ShopReports()
        {
            string permissionType = GetUserPermission("Shop Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                return View(_shop.GetShopReports(userId));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
        public IActionResult CompanyReports()
        {
            string permissionType = GetUserPermission("Company Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View(_company.GetCompanyReports());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
        public IActionResult EmployeeReports()
        {
            string permissionType = GetUserPermission("Employee Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                return View(_employee.GetEmployeeReports(userId));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
        public IActionResult TransactionReports()
        {
            string permissionType = GetUserPermission("Transaction Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
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
                return View(_orders.GetTransactionReports(userId));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
        public IActionResult ProductReports()
        {
            string permissionType = GetUserPermission("Product Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
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
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
            
        }
        public IActionResult ActivityLog()
        {
            string permissionType = GetUserPermission("Activity Log");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }
    }
}
