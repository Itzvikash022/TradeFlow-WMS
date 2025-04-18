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
        private readonly IActivityRepository _activity;
        public AdminsController(dbMain context, ISidebarRepository sidebar, IAdminsRepository admins, IUsersRepository users, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export, IActivityRepository activity) : base(sidebar)
        {
            _context = context;
            _admins = admins;
            _users = users;
            _emailSender = emailSender;
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


        [Route("Owners")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Owners");
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

            var dataTable = new DataTable("Owners");
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

            var fileBytes = _export.ExportToExcel(dataTable, "OwnersList");

            if (fileBytes != null)
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string name = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Export Owners List";
                string desc = $"{name} exported Admin list";

                _activity.AddNewActivity(userId, roleId, type, desc);
            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AdminList.xlsx");
        }



        [HttpGet]
        public IActionResult Details(int id)
        {
            string permissionType = GetUserPermission("Owners");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblUser admin = _context.TblUsers.FirstOrDefault(x => x.UserId == id);
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
                if (shop != null)
                {
                    shop.IsDeleted = true;
                    shop.IsActive = false;
                    _context.TblShops.Update(shop);
                }

                _context.TblUsers.Update(admin);
                _context.SaveChanges();

                //Sending email after deletion
                string subject = "Account Deletion Notification";
                string emailBody = @"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <title>Account Deletion Notification</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                            <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                <h2 style='color: #333;'>Account Deletion Notification</h2>
                                <p style='font-size: 16px; color: #555;'>Dear [User's Name],</p>
                                <p style='font-size: 16px; color: #555;'>We regret to inform you that your account has been successfully deleted. If you have any questions or concerns regarding this process, please do not hesitate to contact our support team. We're here to assist you with any queries you may have.</p>
                                <p style='font-size: 16px; color: #555;'>Thank you for your understanding.</p>
                                <p style='font-size: 16px; color: #555;'>Best regards,</p>
                                <p style='font-size: 16px; color: #555;'>TradeFlow Support Team</p>
                                <p style='font-size: 14px; color: #888;'>If you believe this is an error, please reach out to support immediately.</p>
                                <div style='margin-top: 30px; font-size: 12px; color: #aaa;'>
                                    <p>If you did not request this action, please contact our support team immediately.</p>
                                </div>
                            </div>
                        </body>
                        </html>";
                _emailSender.SendEmailAsync(admin.Email, subject, emailBody);

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Owner Delete";
                string desc = $"{userName} deleted {admin.Username}";

                _activity.AddNewActivity(userId, roleId, type, desc);
                // If successful, redirect to Index
                return Json(new { success = true, message = "Owner deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting Owner: " + ex.Message);
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
                string type = "";
                var admin = _context.TblUsers.Find(id);
                var shop = _context.TblShops.FirstOrDefault(x => x.AdminId == id);
                string msg = "";
                if (admin.IsActive == false)
                {
                    admin.IsActive = true;
                    if (roleId <= 2 && shop != null)
                    {
                        shop.IsActive = true;
                    }
                    msg = "User UnRestricted successfully.";
                    type = "Unrestrict";
                }
                else
                {
                    admin.IsActive = false;

                    if (roleId <= 2 && shop != null)
                    {
                        shop.IsActive = false;
                    }
                    msg = "User Restricted successfully.";
                    type = "Restrict";
                }

                if (roleId <= 2 && shop != null)
                {
                    _context.TblShops.Update(shop);
                }
                _context.TblUsers.Update(admin);
                _context.SaveChanges();

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
                string desc = $"{userName} {type}ed {admin.Username}";

                _activity.AddNewActivity(userId, roleId, $"{type} User", desc); 

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
            string verifier = ((int)HttpContext.Session.GetInt32("UserId")).ToString();
            var result = await _admins.UpdateStatus(UserId, Status, verifier, remark);

            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            int Id = (int)HttpContext.Session.GetInt32("UserId");
            string userName = _context.TblUsers.Where(x => x.UserId == Id).Select(y => y.Username).FirstOrDefault();
            string shopKeeperName = _context.TblUsers.Where(x => x.UserId == UserId).Select(y => y.Username).FirstOrDefault();
            string type = "", desc = "";
            if (Status)
            {
                type = "Application Approval";
                desc = $"{userName} Approved {shopKeeperName}'s application request";
            }
            else
            {
                type = "Application Rejection";
                desc = $"{userName} rejected {shopKeeperName}'s application request";
            }

            _activity.AddNewActivity(Id, roleId, type, desc);
                return Json(result);
        }

        //[Route("Admins/Save")]
        [HttpGet]
        public async Task<IActionResult> SaveAdmin(int? id, string? from)
        {
            // Determine role type dynamically
            string roleToCheck = from == "admin" ? "Owners" : "Employees";
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
                if (user.UserId > 0)
                {
                    TblUser resData = _admins.checkExistence(user.Username, user.Email, user.UserId);
                    if (resData != null)
                    {
                        if (resData.Email == user.Email)
                        {
                            return Json(new { success = false, message = "Email already exists - edit" });
                        }
                        if (resData.Username == user.Username)
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
                int ResId = (int)HttpContext.Session.GetInt32("UserId");
                int verifier = ResId;
                user.VerifiedBy = verifier.ToString();
                string type = "Owner";
                if (user.RoleId > 2)
                {
                    user.AdminRef = verifier;
                    type = "Employee";
                }
                var res = await _admins.SaveUsers(user);


                string activityDesc = "", activityType = "";
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string username = _context.TblUsers.Where(x => x.UserId == ResId).Select(y => y.Username).FirstOrDefault();

                if (id > 0)
                {
                    if (((dynamic)res).success)
                    {
                        TempData["saveAdmin-toast"] = $"{type} Updated Successfully";
                        TempData["saveAdmin-toastType"] = "success";

                        string subject = $"{type} Account Update Notification";
                        string body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Account Updated</title>
                            </head>
                            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                                <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                    <h2 style='color: #333;'>Account Successfully Updated</h2>
                                    <p style='font-size: 16px; color: #555;'>Hello {user.FirstName},</p>
                                    <p style='font-size: 16px; color: #555;'>We would like to inform you that your account has been successfully updated by the SuperAdmin. You can now access your account with the updated information.</p>
                                    <p style='font-size: 16px; color: #555;'>Please note that while some information has been updated, your username and email have been sent to you regardless of any changes made. You can log in using the credentials provided below:</p>
            
                                    <p style='font-size: 16px; color: #555;'>
                                        <strong>Username:</strong> {user.Username}<br>
                                        <strong>Email:</strong> {user.Email}<br>
                                        <strong>Password:</strong> Your existing password remains the same.
                                    </p>
            
                                    <p style='font-size: 16px; color: #555;'>Please keep in mind that this is sensitive information. We advise you not to share your credentials with anyone else for your account's security.</p>

                                    <p style='font-size: 16px; color: #555;'>Thank you for being a valued member.</p>

                                    <p style='font-size: 16px; color: #555;'>Best regards,</p>
                                    <p style='font-size: 16px; color: #555;'>[Your Company Name] Support Team</p>
                                    <div style='margin-top: 30px; font-size: 12px; color: #aaa;'>
                                        <p>If you did not request this update, please contact our support team immediately.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";
                        _emailSender.SendEmailAsync(user.Email, subject, body);

                        activityDesc = $"{username} updated {user.Username}'s details";
                        activityType = $"{type} Update";
                    }
                }
                else
                {
                    if (((dynamic)res).success)
                    {
                        TempData["saveAdmin-toast"] = $"{type} Added Successfully";
                        TempData["saveAdmin-toastType"] = "success";

                        string subject = $"{type} Account Creation Confirmation";
                        string body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Account Created</title>
                            </head>
                            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                                <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                    <h2 style='color: #333;'>Account Successfully Created</h2>
                                    <p style='font-size: 16px; color: #555;'>Hello {user.FirstName},</p>
                                    <p style='font-size: 16px; color: #555;'>Welcome to our WMS Application! We are pleased to inform you that your account has been successfully created.</p>
                                    <p style='font-size: 16px; color: #555;'>You can now access your account using the credentials provided below:</p>
            
                                    <p style='font-size: 16px; color: #555;'>
                                        <strong>Username:</strong> {user.Username}<br>
                                        <strong>Email:</strong> {user.Email}<br>
                                        <strong>Password:</strong> {password}
                                    </p>
            
                                    <p style='font-size: 16px; color: #555;'>Once logged in, you will be prompted to fill out additional information to complete your profile setup.</p>

                                    <p style='font-size: 16px; color: #555;'>Please note that these credentials are sensitive. We advise you not to share your username and password with anyone else for your account's security.</p>

                                    <p style='font-size: 16px; color: #555;'>Thank you for joining us. We look forward to having you as a valued user of our platform.</p>

                                    <p style='font-size: 16px; color: #555;'>Best regards,</p>
                                    <p style='font-size: 16px; color: #555;'>[Your Company Name] Support Team</p>

                                    <div style='margin-top: 30px; font-size: 12px; color: #aaa;'>
                                        <p>If you did not request this account creation, please contact our support team immediately.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";
                        _emailSender.SendEmailAsync(user.Email, subject, body);

                        activityDesc = $"{username} added new {type} named : {user.Username}";
                        activityType = $"Add new {type}";
                    }
                }

                _activity.AddNewActivity(ResId, roleId, activityType, activityDesc);
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
