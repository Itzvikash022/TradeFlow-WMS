using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public AdminsController(dbMain context, ISidebarRepository sidebar, IAdminsRepository admins, IUsersRepository users, IEmailSenderRepository emailSender) : base(sidebar)
        {
            _context = context;
            _admins = admins;
            _users = users;
            _emailSender = emailSender;
        }
        [Route("Admins")]
        public async Task<IActionResult> Index()
        {
            return View(await _admins.GetAllAdminsData());
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            TblUser admin= _context.TblUsers.FirstOrDefault(x => x.UserId == id);
            TblAdminInfo adminInfo = _context.TblAdminInfos.FirstOrDefault(x => x.AdminId == id);
            TblShop shopInfo = _context.TblShops.FirstOrDefault(x => x.AdminId == id);

            ViewBag.UserDetails = admin;
            ViewBag.AdminInfo = adminInfo;
            ViewBag.ShopInfo = shopInfo;
            return View();
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
                _context.TblUsers.Remove(admin);
                _context.SaveChanges();

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
        public async Task<IActionResult> SaveAdmin(int? id)
        {
            TblUser model = new TblUser();
            ViewBag.Roles = await _admins.GetAllRoles();
            if (id > 0)
            {
                model = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == id);
            }
            return View(model);
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
                string verifier = ResId.ToString();
                user.VerifiedBy = verifier;
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
