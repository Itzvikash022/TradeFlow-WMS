using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly dbMain _db;
        private readonly IUsers _users;
        private readonly IEmailSender _emailSender;
        private readonly ILogin _login;
        private readonly IMemoryCache _memoryCache;
            
        public AuthController(ILogger<AuthController> logger, dbMain db, IUsers users, IEmailSender emailSender, ILogin login, IMemoryCache memoryCache)
        {
            _logger = logger;
            _db = db;
            _users = users;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
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
            var res = await _login.TokenSenderViaEmail(user.Email);
            return Ok(res);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Validate the token
            if (string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out object tokenData))
            {
                ViewBag.InvalidToken = true;
                return View();
            }
            ViewBag.InvalidToken = false;
            ViewBag.Token = token;
            _memoryCache.Remove(token);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAction(string PasswordHash)
        {
            string user = HttpContext.Session.GetString("ForgotPassEmail");
            var res = await _login.ResetPassword(user, PasswordHash);
            
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
            var model = new LoginModel();
            if (Request.Cookies.TryGetValue("RememberMe_Email", out string Emailvalue))
            {
                model.EmailOrUsername = Emailvalue;
                model.RememberMe = true;
            }

            //JIC, password is working fine and is being stored in the input field, but due to browser restrictions being type=pass, it can't autofill, changin pass field type=text fixes it.
            //if (Request.Cookies.TryGetValue("RememberMe_Password", out string Passvalue))
            //{
            //    model.Password = Passvalue;
            //    model.RememberMe = true;
            //}

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            const int maxAttempts = 5; // Maximum allowed attempts
            const int lockoutDurationSeconds = 300; // Lockout duration in minutes

            // Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{login.EmailOrUsername}";
            var lockoutKey = $"Lockout_{login.EmailOrUsername}";

            // Check if the user is locked out
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (lockoutEndTime - DateTime.Now).Seconds;
                return Json(new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." });
            }

            var result = await _login.AuthenticateUser(login.EmailOrUsername, login.Password);

            // Set session if login was successful
            if (((dynamic)result).success)
            {
                HttpContext.Session.SetString("Cred", login.EmailOrUsername);

                if (login.RememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7), // Cookie expiration time
                        HttpOnly = true,                  // For security
                        Secure = true                     // Use HTTPS
                    };
                    Response.Cookies.Append("RememberMe_Email", login.EmailOrUsername, options);
                    Response.Cookies.Append("RememberMe_Password", login.Password, options);
                }
                else
                {
                    // Clear cookies if Remember Me is not checked
                    Response.Cookies.Delete("RememberMe_Email");
                    Response.Cookies.Delete("RememberMe_Password");
                }

            }
            else
            {
                // Increment the login attempts
                var attempts = _memoryCache.Get<int>(attemptKey) + 1;
                _memoryCache.Set(attemptKey, attempts, TimeSpan.FromSeconds(lockoutDurationSeconds));
                // Lockout the user if max attempts reached
                if (attempts >= maxAttempts)
                {
                    _memoryCache.Set(lockoutKey, DateTime.Now.AddSeconds(lockoutDurationSeconds), TimeSpan.FromSeconds(lockoutDurationSeconds));
                    _memoryCache.Remove(attemptKey);
                    return Json(new { success = false, message = $"Account is locked. Try again in {lockoutDurationSeconds} seconds." });
                }
                return Json(new { success = false, message = $"Invalid credentials. You have {maxAttempts - attempts} attempts left." });
            }

            _memoryCache.Remove(attemptKey);
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

        public IActionResult AdminDoc()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminDoc(AdminInfo info)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            if (id != null)
            {
                return Json(await _users.SaveAdminDoc(info, id));
            }
            else
            {
                return Json(new { success = false, message = "Owner Id not found" });
            }
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
                String email = HttpContext.Session.GetString("UserEmail");
                if (await _users.IsEmailExists(email))
                {
                    if (await _users.OtpVerification(user.Otp))
                    {
                        await _users.updateStatus(email);
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

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.OtpExpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp);
                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }
        






            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
