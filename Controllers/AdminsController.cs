using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Composition;
using System.Data;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class AdminsController : BaseController
    {
        private readonly dbMain _context;
        private readonly IUsersRepository _users;
        private readonly IAdminsRepository _admins;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IPermisionHelperRepository _permission;
        private readonly IExportServiceRepository _export;
        public AdminsController(dbMain context, ISidebarRepository sidebar, IAdminsRepository admins, IUsersRepository users, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export) : base(sidebar)
        {
            _context = context;
            _admins = admins;
            _users = users;
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


        [Route("Admins")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Admins");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View(await _admins.GetAllAdminsData());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public async Task<IActionResult> ExportAdminList()
        {
            List<TblUser> adminList = await _admins.GetAllAdminsData();

            var dataTable = new DataTable("Admins");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Full Name"),
                new DataColumn("User Name"),
                new DataColumn("Email"),
                new DataColumn("Joined On"),
                new DataColumn("PhoneNumber"),
                new DataColumn("DOB"),
                new DataColumn("IsVerified"),
                new DataColumn("Current Status")
            });

            foreach (var admin in adminList)
            {
                dataTable.Rows.Add(admin.FirstName + " " + admin.LastName, admin.Username, admin.Email, admin.CreatedAt, admin.PhoneNumber, admin.DateOfBirth, admin.IsVerified, admin.VerificationStatus);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "AdminList");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AdminList.xlsx");
        }



        [HttpGet]
        public IActionResult Details(int id)
        {
            string permissionType = GetUserPermission("Admins");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblUser admin= _context.TblUsers.FirstOrDefault(x => x.UserId == id);
                TblAdminInfo adminInfo = _context.TblAdminInfos.FirstOrDefault(x => x.AdminId == id);
                TblShop shopInfo = _context.TblShops.FirstOrDefault(x => x.AdminId == id);

                ViewBag.UserDetails = admin;
                ViewBag.AdminInfo = adminInfo;
                ViewBag.ShopInfo = shopInfo;
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public IActionResult DeleteAdmin(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid admin ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var admin = _context.TblUsers.Find(id);
                var shop = _context.TblShops.FirstOrDefault(x => x.AdminId == id);
                
                admin.IsDeleted = true;
                admin.IsActive = false;
                if(shop != null)
                {
                    shop.IsDeleted = true;
                    shop.IsActive = false;
                    _context.TblShops.Update(shop);
                }

                _context.TblUsers.Update(admin);
                _context.SaveChanges();

                //Sending email after deletion
                string subject = "Account Deleted!! heehehehehehe";
                string body = "I'm sorry to inform you but your account has been terminated, please contact the support team if you have query regarding it";
                _emailSender.SendEmailAsync(admin.Email, subject, body);

                // If successful, redirect to Index
                return Json(new { success = true, message = "Admin deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting admin: " + ex.Message);
                return Json(new { success = false, message = "Error deleting admin." });
            }
        }
        [HttpPost]
        [Route("Admins/RestrictionStatus")]
        public IActionResult RestrictionStatus(int id)
        {
            int roleId = _context.TblUsers.Where(x => x.UserId == id).Select(y => y.RoleId).FirstOrDefault();
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid admin ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var admin = _context.TblUsers.Find(id);
                var shop = _context.TblShops.FirstOrDefault(x => x.AdminId == id);
                string msg = "";
                if(admin.IsActive == false)
                {
                    admin.IsActive = true;
                    if(roleId <= 2)
                    {
                        shop.IsActive = true;
                    }
                    msg = "User UnRestricted successfully.";
                }
                else
                {
                    admin.IsActive = false;
                    if (roleId <= 2)
                    {
                        shop.IsActive = false;
                    }
                    msg = "User Restricted successfully.";
                }

                if(roleId <= 2)
                {
                    _context.TblShops.Update(shop);
                }
                _context.TblUsers.Update(admin);
                _context.SaveChanges();

                // If successful, redirect to Index
                return Json(new { success = true, message = msg });
            }
            catch (Exception ex)

            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error restricting user: " + ex.Message);
                return Json(new { success = false, message = "Error restricting user." });
            }
        }

        //public IActionResult 

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int UserId, bool Status, string remark)
        {   
            string verifier = ((int) HttpContext.Session.GetInt32("UserId")).ToString();
            var result = await _admins.UpdateStatus(UserId, Status, verifier, remark);
            return Json(result);
        }

        //[Route("Admins/Save")]
        [HttpGet]
        public async Task<IActionResult> SaveAdmin(int? id, string? from)
        {
            // Determine role type dynamically
            string roleToCheck = from == "admin" ? "Admins" : "Employees";
            string permissionType = GetUserPermission(roleToCheck);
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblUser model = new TblUser();
                ViewBag.Roles = await _admins.GetAllRoles();
                if (id > 0)
                {
                    model = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == id);
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAdmin(TblUser user)
        {
            try
            {
                if(user.UserId > 0)
                {
                    TblUser resData = _admins.checkExistence(user.Username, user.Email, user.UserId);
                    if(resData != null)
                    {
                        if(resData.Email == user.Email)
                        {
                            return Json(new { success = false, message = "Email already exists - edit" });
                        }
                        if(resData.Username == user.Username)
                        {
                            return Json(new { success = false, message = "Username already exists - edit" });
                        }
                    }
                }
                else 
                { 
                    if (await _users.IsUsernameExists(user.Username))
                    {
                        return Json(new { success = false, message = "Username already exists" });
                    }
                    if (await _users.IsEmailExists(user.Email))
                    {
                        return Json(new { success = false, message = "Email already exists" });
                    }
                }
                string password = user.PasswordHash;
                int id = user.UserId;

                int ResId = (int) HttpContext.Session.GetInt32("UserId");
                int verifier = ResId;
                user.VerifiedBy = verifier.ToString();
                string type = "Admin";
                if(user.RoleId > 2)
                {
                    user.AdminRef = verifier;
                    type = "Employee";
                }
                var res = await _admins.SaveUsers(user);

                if(id > 0)
                {
                    if (((dynamic)res).success)
                    {
                        string subject = $"{type} account has been updated";
                        string body = $"Hello there {user.FirstName}, your account has been successfully update by the SuperAdmin and you can access your account now, some information has been update and your username and email has been mailed regardless of any changes, you can now login under the given credentials. UserName : {user.Username}, Email : {user.Email}, Password : you old same pass. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                else
                {
                    if (((dynamic)res).success)
                    {
                        string subject = $"{type} account has been created";
                        string body = $"Hello there {user.FirstName}, welcome to our WMS Application, you account has been successfully created and you can access your account under the given credentials. UserName : {user.Username}, Email : {user.Email}, Password : {password} and after login, you can fill out your extra informations. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                    return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });
            }
        }
    }
}
