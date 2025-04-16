using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
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
        private readonly IActivityRepository _activity;

        public ReportsController(ISidebarRepository sidebar, dbMain context, IAdminsRepository admin, IShopRepository shop, ICompanyRepository company, IEmployeeRepository employee, IOrdersRepository orders, IProductRepository product, IPermisionHelperRepository permission, IExportServiceRepository export, IActivityRepository activity) : base(sidebar)
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
            _activity = activity;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
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

            if (fileBytes != null)
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Admin Reports Export";
                string desc = $"{userName} exported Admin Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }

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

            if (fileBytes != null)
            {
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Shop Reports Export";
                string desc = $"{userName} exported Shop Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }

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

            if (fileBytes != null)
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Company Reports Export";
                string desc = $"{userName} exported Company Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }


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

            if (fileBytes != null)
            {
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Employee Reports Export";
                string desc = $"{userName} exported Employee Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }

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
            int userId = 0, id = 0;
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if (roleId == 5)
            {
                userId = (int)HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                userId = (int)HttpContext.Session.GetInt32("ShopId");
                id = (int)HttpContext.Session.GetInt32("UserId");

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


            if (fileBytes != null)
            {
                string name = "";
                if (roleId == 5)
                {
                    name = _context.TblCompanies.Where(x => x.CompanyId == userId).Select(y => y.CompanyName).FirstOrDefault();
                }
                else
                {
                    name = _context.TblUsers.Where(x => x.UserId == id).Select(y => y.Username).FirstOrDefault();
                }

                string type = "Transaction Reports Export";
                string desc = $"{name} exported Transaction Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionReports.xlsx");
        }


        public IActionResult ProductReports()
        {
            string permissionType = GetUserPermission("Product Reports");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int userId = 0, id = 0;
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                if (roleId == 5)
                {
                    userId = (int)HttpContext.Session.GetInt32("CompanyId");
                }
                else
                {
                    userId = (int)HttpContext.Session.GetInt32("ShopId");
                    id = (int)HttpContext.Session.GetInt32("UserId");
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
            int userId = 0, id = 0;
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if (roleId == 5)
            {
                userId = (int)HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                userId = (int)HttpContext.Session.GetInt32("ShopId");
                id = (int)HttpContext.Session.GetInt32("UserId");
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

            if (fileBytes != null)
            {
                string name = "";

                if (roleId == 5)
                {
                    name = _context.TblCompanies.Where(x => x.CompanyId == userId).Select(y => y.CompanyName).FirstOrDefault();
                }
                else
                {
                    name = _context.TblUsers.Where(x => x.UserId == id).Select(y => y.Username).FirstOrDefault();
                }

                string type = "Product Reports Export";
                string desc = $"{name} exported Product Reports";
                _activity.AddNewActivity(userId, roleId, type, desc);
            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductReports.xlsx");
        }

        public IActionResult ActivityLog()
        {
            string permissionType = GetUserPermission("Activity Log");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {

                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                List<TblActivityLog> activities = new List<TblActivityLog>();

                if (roleId == 5)
                {
                    int compId = (int)HttpContext.Session.GetInt32("CompanyId");
                    activities = _context.TblActivityLogs.Where(y => y.UserId == compId).OrderByDescending(x => x.Timestamp).ToList();
                }
                else
                {
                    int userId = (int)HttpContext.Session.GetInt32("UserId");

                    List<int> relatedUserIds = new List<int>();

                    if (roleId <= 2) // Admin or SuperAdmin
                    {
                        // Admin is logged in — get all users under them
                        relatedUserIds = _context.TblUsers
                                            .Where(u => u.AdminRef == userId)
                                            .Select(u => u.UserId)
                                            .ToList();

                        // Include self
                        relatedUserIds.Add(userId);
                    }
                    else
                    {
                        // Employee is logged in — get their admin ID first
                        int? adminRef = _context.TblUsers
                                            .Where(u => u.UserId == userId)
                                            .Select(u => u.AdminRef)
                                            .FirstOrDefault();

                        if (adminRef != null)
                        {
                            relatedUserIds = _context.TblUsers
                                                .Where(u => u.AdminRef == adminRef)
                                                .Select(u => u.UserId)
                                                .ToList();

                            relatedUserIds.Add((int)adminRef); // include their admin
                        }
                        else
                        {
                            relatedUserIds.Add(userId); // fallback, just in case
                        }
                    }

                    // Finally, fetch all logs for those related IDs
                    activities = _context.TblActivityLogs
                                        .Where(log => relatedUserIds.Contains(log.UserId))
                                        .OrderByDescending(log => log.Timestamp)
                                        .ToList();

                }


                List<TblActivityLog> activityLog = new List<TblActivityLog>();

                foreach (var activity in activities)
                {
                    activityLog.Add(new TblActivityLog
                    {
                        ActivityType = activity.ActivityType,
                        Description = activity.Description,
                        Role = activity.Role,
                        Timestamp = activity.Timestamp,
                        UserId = activity.UserId,
                        LogId = activity.LogId,
                        ImagePath = activity.Role == 5
                            ? _context.TblCompanies.Where(x => x.CompanyId == activity.UserId).Select(y => y.CompanyLogo).FirstOrDefault()
                            : _context.TblUsers.Where(x => x.UserId == activity.UserId).Select(y => y.ProfileImgPath).FirstOrDefault()
                    });
                }

                return View(activityLog);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }




    }
}
