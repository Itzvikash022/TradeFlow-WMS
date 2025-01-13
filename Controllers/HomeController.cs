using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly dbMain _db;
        private readonly UsersInterface _users;
        private readonly EmailSenderInterface _emailSender;
        public HomeController(ILogger<HomeController> logger, dbMain db, UsersInterface users, EmailSenderInterface emailSender)
        {
            _logger = logger;
            _db = db;
            _users = users;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult OtpCheck()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OtpCheck(User user)
        {
            try
            {
                if (await _users.IsEmailExists(user.Email))
                {
                    if (await _users.OtpVerification(user.Otp))
                    {
                        await _users.updateStatus(user.Email);
                        return Json(new { success = true, message = "OTP verified successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "OTP verification failed" });
                    }
                }
                return Json(new { success = false, message = "Email not found" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });

            }
        }

        public async Task<IActionResult> Register()
        {
            ViewBag.Designation = await _users.GetDesignations();
            ViewBag.Admins = await _users.GetAdminUsernames();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] User user)
        {
            try
             {
                user.Role = "Admin";
                if (await _users.IsUsernameExists(user.Username))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }
                if (await _users.IsEmailExists(user.Email))
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                TempData["UserEmail"] = user.Email;

                return Json(await _users.SaveUsers(user));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });

            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
