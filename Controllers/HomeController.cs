using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly LoginInterface _login;
        private readonly IMemoryCache _memoryCache;
        private readonly ForgotPasswordInterface _forgot;
            
        public HomeController(ILogger<HomeController> logger, dbMain db, UsersInterface users, EmailSenderInterface emailSender, LoginInterface login, IMemoryCache memoryCache, ForgotPasswordInterface forgot)
        {
            _logger = logger;
            _db = db;
            _users = users;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
            _forgot = forgot;
        }

        public IActionResult Index()
        {
            Console.WriteLine("ehehehhhhhehhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");
            Console.WriteLine(HttpContext.Session.GetString("Cred"));
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(User user)
        {
            HttpContext.Session.SetString("ForgotPassEmail", user.Email);
            var res = await _forgot.TokenSenderViaEmail(user.Email);
            return Ok(res);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Validate the token
            if (string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out object tokenData))
            {
                return Json(new { success = false, message = "Token is Invalid or Expired, please send another link" });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAction(string PasswordHash)
        {

            string user = HttpContext.Session.GetString("ForgotPassEmail");
            var res = await _forgot.ResetPassword(user, PasswordHash);
            return Ok(res);
        }
        public IActionResult OtpCheck()
        {
            return View();
        }
        public IActionResult MoreDetails()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            var result = await _login.AuthenticateUser(login.EmailOrUsername, login.Password);

            // Set session if login was successful
            if (((dynamic)result).success)
            {
                HttpContext.Session.SetString("Cred", login.EmailOrUsername);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> MoreDetails(User user)
        {
            int id = (int) await _users.GetUserIdByEmail(user.Email);
            HttpContext.Session.SetInt32("UserId", id);
            return Json(await _users.SaveMoreDetails(user));
        }
        public IActionResult ShopDetails()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShopDetails(Shop shop)
        {
            int id = (int) HttpContext.Session.GetInt32("UserId");
            if(id != null)
            {
                return Json(await _users.SaveShopDetails(shop, id));
            }
            else
            {
                return Json(new { success = false, message = "Owner Id not found" });
            }
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
            //ViewBag.Designation = await _users.GetDesignations();
            //ViewBag.Admins = await _users.GetAdminUsernames();
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

                //TempData["UserEmail"] = user.Email;
                HttpContext.Session.SetString("UserEmail", user.Email);
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
