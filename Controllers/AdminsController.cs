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

        //public IActionResult 

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int UserId, bool Status)
        {   
            string verifier = ((int) HttpContext.Session.GetInt32("UserId")).ToString();
            var result = await _admins.UpdateStatus(UserId, Status, verifier);
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
                            return Json(new { success = false, message = "Email already exists" });
                        }
                        if(resData.Username == user.Username)
                        {
                            return Json(new { success = false, message = "Username already exists" });
                        }
                    }
                }
                else 
                { 
                    if (await _users.IsUsernameExists(user.Username))
                    {
                        return Json(new { success = false, message = "Username already exists - edit" });
                    }
                    if (await _users.IsEmailExists(user.Email))
                    {
                        return Json(new { success = false, message = "Email already exists - edit" });
                    }
                }
                string username = user.Username;
                string firstname = user.FirstName;
                string password = user.PasswordHash;
                string email = user.Email;
                int id = user.UserId;

                int ResId = (int) HttpContext.Session.GetInt32("UserId");
                string verifier = ResId.ToString();
                user.VerifiedBy = verifier;
                if(user.RoleId > 2)
                {
                    user.AdminRef = verifier;
                }
                var res = await _admins.SaveUsers(user);

                if(id > 0)
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Admin account has been updated by the SuperAdmin";
                        string body = $"Hello there {user.FirstName}, your account has been successfully update by the SuperAdmin and you can access your account now, some information has been update and your username and email has been mailed regardless of any changes, you can now login under the given credentials. UserName : {user.Username}, Email : {user.Email}, Password : you old same pass. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
                        _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }
                else
                {
                    if (((dynamic)res).success)
                    {
                        string subject = "Admin account has been created by the SuperAdmin";
                        string body = $"Hello there {firstname}, welcome to our WMS Application, you account has been successfully created and you can access your account under the given credentials. UserName : {username}, Email : {email}, Password : {password} and after login, you can fill out your extra informations. But keep in mind, this is a sensitive information, so plz don't share it with anyone else. Thank you";
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
