using Microsoft.AspNetCore.Mvc;
using System.Composition;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
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
        private readonly IImportServiceRepository _import;
        private readonly IActivityRepository _activity;
        public EmployeeController(ISidebarRepository sidebar, dbMain context, IEmployeeRepository employee, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export, IImportServiceRepository import, IActivityRepository activity) : base(sidebar)
        {
            _employee = employee;
            _context = context;
            _emailSender = emailSender;
            _permission = permission;
            _export = export;
            _import = import;
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
            List<TblUser> employeeList = await _employee.GetAllEmployees(id);

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

            if (fileBytes != null)
            {
                //rechecking the session not reusing the upper ones cuz, it was a mess changing bit might crash something so im not touching it

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string name = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Export Employee List";
                string desc = $"{name} exported Employee list";

                _activity.AddNewActivity(userId, roleId, type, desc);
            }

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
                string body = "I'm sorry to inform you but your account has been terminated, please contact the support team if you have query regarding it";
                _emailSender.SendEmailAsync(emp.Email, subject, body);

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string type = "Employee Delete";
                string desc = $"{userName} deleted {emp.Username}'s account";


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


        public IActionResult DownloadSampleFile()
        {
            var fileBytes = _import.GenerateEmployeeSampleExcel();

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleFile.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSampleFile(IFormFile excelFile, IFormFile imageZip)
        {
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (excelFile == null || imageZip == null)
                return BadRequest("Please upload both Excel file and Image ZIP folder.");

            if (!imageZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .zip files are allowed for image upload." });
            }
            // Extract ZIP file
            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\UserUploads");
            Directory.CreateDirectory(imageFolderPath);

            using (var zipStream = new MemoryStream())
            {
                await imageZip.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string extension = Path.GetExtension(entry.Name).ToLower();

                        // ❌ Reject non-image files
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, message = "only image files are allowed inside the zip" });
                        }

                        // ✅ Move valid images
                        var filePath = Path.Combine(imageFolderPath, entry.Name);
                        if (!System.IO.File.Exists(filePath))
                        {
                            entry.ExtractToFile(filePath);
                        }
                    }
                }
            }



            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "File is empty." });
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .xlsx files are allowed." });
            }


            try
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                var fileBytes = await _import.EmployeeProcessImport(excelFile, userId);

                // Convert to Base64 to store temporarily
                string fileBase64 = Convert.ToBase64String(fileBytes);

                if (fileBase64 != null)
                {
                    string type = "Mass Import Employees";
                    int count = (int)HttpContext.Session.GetInt32("ImportedEmployeeCount");
                    string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(s => s.Username).FirstOrDefault();
                    string desc = $"{userName} Mass imported {count} new Employees";

                    _activity.AddNewActivity(userId, roleId, type, desc);
                }
                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully!",
                    fileData = fileBase64,
                    fileName = "OutputOfSampleFile.xlsx"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

    }
}
