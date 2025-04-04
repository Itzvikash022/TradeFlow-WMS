using Microsoft.AspNetCore.Mvc;
using System.Composition;
using System.Data;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeRepository _employee;
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IPermisionHelperRepository _permission;
        private readonly IExportServiceRepository _export;
        public EmployeeController(ISidebarRepository sidebar,dbMain context, IEmployeeRepository employee, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export) : base(sidebar)
        {
            _employee = employee;
            _context = context;
            _emailSender = emailSender;
            _permission = permission;
            _export = export;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        [Route("Employees")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Employees");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int id = ((int)HttpContext.Session.GetInt32("UserId"));
                return View(await _employee.GetAllEmployees(id));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }

        public async Task<IActionResult> ExportEmployeeList()
        {
            int id = ((int)HttpContext.Session.GetInt32("UserId"));
            List<TblUser> employeeList =await _employee.GetAllEmployees(id);

            var dataTable = new DataTable("Employees");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Full Name"),
                new DataColumn("User Name"),
                new DataColumn("Email"),
                new DataColumn("Joined On"),
                new DataColumn("PhoneNumber"),
                new DataColumn("DOB"),
                new DataColumn("Designation"),
                new DataColumn("IsVerified")
            });

            foreach (var employee in employeeList)
            {
                dataTable.Rows.Add(employee.FirstName + employee.LastName, employee.Username, employee.Email, employee.CreatedAt, employee.PhoneNumber, employee.DateOfBirth, employee.Designation, employee.IsVerified);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "EmployeeList");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeList.xlsx");
        }


        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid Employee ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var emp = _context.TblUsers.Find(id);
                emp.IsDeleted = true;
                emp.IsActive = false;
                _context.TblUsers.Update(emp);
                _context.SaveChanges();


                //Sending email after deletion
                string subject = "Account Deleted!! heehehehehehe";
                string body= "I'm sorry to inform you but your account has been terminated, please contact the support team if you have query regarding it";
                _emailSender.SendEmailAsync(emp.Email, subject, body);

                // If successful, redirect to Index
                return Json(new { success = true, message = "Employee deleted successfully." });

            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting admin: " + ex.Message);
                return Json(new { success = false, message = "Error deleting admin." });
            }
        }
    }
}
    