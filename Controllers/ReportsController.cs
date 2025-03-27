using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Data;
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
        private readonly IExportServiceRepository _export;

        public ReportsController(ISidebarRepository sidebar, dbMain context, IAdminsRepository admin, IShopRepository shop, ICompanyRepository company, IEmployeeRepository employee, IOrdersRepository orders, IProductRepository product, IPermisionHelperRepository permission, IExportServiceRepository export) : base(sidebar)
        {
            _context = context;
            _admin = admin;
            _shop = shop;
            _company = company;
            _employee = employee;
            _orders = orders;
            _product = product;
            _permission = permission;
            _export = export;
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

        public async Task<IActionResult> ExportAdminReports()
        {
            var adminReports = _admin.GetAdminReports();

            var dataTable = new DataTable("Admins");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Admin ID"),
        new DataColumn("Admin Name"),
        new DataColumn("Email"),
        new DataColumn("Registered On"),
        new DataColumn("ShopDetails"),
        new DataColumn("Documents"),
        new DataColumn("Status"),
        new DataColumn("Employees")
            });

            foreach (var admin in adminReports)
            {
                dataTable.Rows.Add(admin.AdminId, admin.FullName, admin.Email, admin.RegisteredOn, admin.ShopDetails, admin.Documents, admin.Status, admin.Employees);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "AdminReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AdminReports.xlsx");
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
            
        public async Task<IActionResult> ExportShopReports()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var shopReports = _shop.GetShopReports(userId);

            var dataTable = new DataTable("Shops");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Shop ID"),
        new DataColumn("Shop Name"),
        new DataColumn("Owner Name"),
        new DataColumn("Registered On"),
        new DataColumn("Location"),
        new DataColumn("Order Count"),
        new DataColumn("Status"),
            });

            foreach (var shop in shopReports)
            {
                dataTable.Rows.Add(shop.ShopId, shop.ShopName, shop.ShopOwner, shop.CreatedAt, shop.City + shop.State, shop.OrderCount, shop.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "ShopReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ShopReports.xlsx");
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

        public async Task<IActionResult> ExportCompanyReports()
        {
            var companyReports = _company.GetCompanyReports();

            var dataTable = new DataTable("Company");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Company ID"),
        new DataColumn("Company Name"),
        new DataColumn("Email"),
        new DataColumn("Registered On"),
        new DataColumn("Location"),
        new DataColumn("Order Count"),
        new DataColumn("Status"),
            });

            foreach (var company in companyReports)
            {
                dataTable.Rows.Add(company.CompanyId, company.CompanyName, company.Email, company.CreatedAt, company.City + company.State, company.OrderCount, company.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "CompanyReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CompanyReports.xlsx");
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

        public async Task<IActionResult> ExportEmployeeReports()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var employeeReports = _employee.GetEmployeeReports(userId);

            var dataTable = new DataTable("Employee");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Employee ID"),
        new DataColumn("Employee Name"),
        new DataColumn("Email"),
        new DataColumn("Joining Date"),
        new DataColumn("Head"),
        new DataColumn("Designation"),
        new DataColumn("ShopName"),
        new DataColumn("Restricted"),
            });

            foreach (var employee in employeeReports)
            {
                dataTable.Rows.Add(employee.UserId, employee.FirstName + employee.LastName, employee.Email, employee.CreatedAt, employee.Head, employee.Designation, employee.ShopName, employee.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "EmployeeReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReports.xlsx");
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


        public async Task<IActionResult> ExportTransactionReports()
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
            var transactionReports = _orders.GetTransactionReports(userId);

            var dataTable = new DataTable("Employee");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Transaction ID"),
        new DataColumn("Transaction Date"),
        new DataColumn("Type"),
        new DataColumn("Payment Mode"),
        new DataColumn("Amount"),
        new DataColumn("Order ID"),
        new DataColumn("Order Date"),
        new DataColumn("Order Status"),
            });

            foreach (var transaction in transactionReports)
            {
                dataTable.Rows.Add(transaction.TransactionId, transaction.TransactionDate, transaction.OrderType, transaction.TransactionType, transaction.Amount, transaction.OrderId, transaction.OrderDate, transaction.OrderStatus);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "TransactionReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionReports.xlsx");
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


        public async Task<IActionResult> ExportProductReports()
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
            var productReports = _product.GetProductsReports(userId, roleId);

            var dataTable = new DataTable("Product");
            dataTable.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Product ID"),
        new DataColumn("Company Name"),
        new DataColumn("Bought On"),
        new DataColumn("Manufacturer"),
        new DataColumn("Quantity"),
        new DataColumn("Bought Price"),
        new DataColumn("Shop Price"),
        new DataColumn("Sales Count"),
            });

            foreach (var product in productReports)
            {
                dataTable.Rows.Add(product.ProductId, product.CompanyName, product.BoughtOn, product.Manufacturer, product.ProductQty, product.PricePerUnit, product.ShopPrice, product.SalesCount);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "ProductReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductReports.xlsx");
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
