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
        private readonly dbMain _context;
        private readonly IUsersRepository _users;
        private readonly IEmailSenderRepository _emailSender;
        private readonly ILoginRepository _login;
        private readonly IMemoryCache _memoryCache;
            
        public AuthController(ILogger<AuthController> logger, dbMain context, IUsersRepository users, IEmailSenderRepository emailSender, ILoginRepository login, IMemoryCache memoryCache)
        {
            _logger = logger;
            _context = context;
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
        public async Task<IActionResult> ForgotPassword(TblUser user)
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

        public async Task<IActionResult> MoreDetails()
        {
            var model = new TblUser();
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                model = await _users.GetUserDataByEmail(email);
            }
            return View(model);
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
            string RedirectTo = "Dashboard";

            const int maxAttempts = 5; // Maximum allowed attempts
            const int lockoutDurationSeconds = 60; // Lockout duration in seconds

            // Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{login.EmailOrUsername}";
            var lockoutKey = $"Lockout_{login.EmailOrUsername}";

            // Check if the user is locked out
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return Json(new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." });
            }

            // Authenticate the user
            var result = await _login.AuthenticateUser(login.EmailOrUsername, login.Password);

            // Check if authentication was successful or not
            if (((dynamic)result).success)
            {
                // Successful login
                HttpContext.Session.SetString("Cred", login.EmailOrUsername);

                if (login.RememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7), // Cookie expiration time
                        HttpOnly = true,                  // Secure the cookie
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

                // Fetch user details and set session variables
                string email = await _users.fetchEmail(login.EmailOrUsername);
                HttpContext.Session.SetString("UserEmail", email);

                var data = await _users.GetUserDataByEmail(email);
                int id = data.UserId;
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetInt32("UserRoleId", data.RoleId);

                // Determine redirection based on user verification and shop details
                if (data.RoleId != 1)
                {
                    if (await _users.IsVerified(login.EmailOrUsername))
                    {
                        if (await _users.hasShopDetails(id))
                        {
                            if (!await _users.hasAdminDoc(id))
                            {
                                RedirectTo = "AdminDoc";
                            }
                        }
                        else
                        {
                            RedirectTo = "ShopDetails"; // Redirect to shop details if not filled
                        }
                    }
                    else
                    {
                        RedirectTo = "OtpCheck"; // Redirect to OTP verification if not verified
                    }
                }

                if(RedirectTo == "OtpCheck")
                {
                    var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == email);
                    if (user != null)
                    {
                        user.Otp = _emailSender.GenerateOtp();
                        user.OtpExpiry = DateTime.Now.AddMinutes(5);
                        await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp);
                        await _context.SaveChangesAsync();
                    }

                }

                // Reset login attempts after successful login
                _memoryCache.Remove(attemptKey);

                return Ok(new { success = true, message = "You are successfully logged in", res = RedirectTo });
            }
            else
            {
                // If not successful, return the message from result
                return Json(new { success = false, message = ((dynamic)result).message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MoreDetails(TblUser user)
        {
            var data = await _users.GetUserDataByEmail(user.Email);
            int id = data.UserId;
            HttpContext.Session.SetInt32("UserId", id);
            return Json(await _users.SaveMoreDetails(user));
        }
        public async Task<IActionResult> ShopDetails()
        {
            var model = new TblShop();
            int id = (int) HttpContext.Session.GetInt32("UserId");
            if(id != 0)
            {
                model = await _users.GetShopDataByUserId(id);
            }
            return View(model);
        }

        public async Task<IActionResult> AdminDoc()
        {
            var model = new TblAdminInfo();
            int id = (int)HttpContext.Session.GetInt32("UserId");
            if (id != 0)
            {
                model = await _users.GetAdminDocDetailsById(id);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminDoc(TblAdminInfo info)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            if (id != null)
            {
                string email = HttpContext.Session.GetString("UserEmail");
                var user =await _users.GetUserDataByEmail(email); 

                string adminEmail = "vikash.my022@gmail.com";
                string subject = "New User Registration - Pending Approval";
                string resetUrl = "http://localhost:5026/Admins";
                string body = $"User: {user.Username}<br>Email: {user.Email}<br>Date: {DateTime.UtcNow}<br>Click <a href='{resetUrl}'>here</a> to go to admin panel.";

                await _emailSender.SendEmailAsync(adminEmail, subject, body);
                return Json(await _users.SaveAdminDoc(info, id));
            }
            else
            {
                return Json(new { success = false, message = "Owner Id not found" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ShopDetails(TblShop shop)
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
        public async Task<IActionResult> OtpCheck(TblUser user)
        {
            try
            {
                String email = HttpContext.Session.GetString("UserEmail");
                if (await _users.IsEmailExists(email))
                {
                    if (await _users.OtpVerification(user.Otp))
                    {
                        await _users.updateStatus(email);
                        //_emailSender.SendEmailAsync()
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
        public async Task<IActionResult> Register([FromForm] TblUser user)
        {
            try
             {
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
                var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.OtpExpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp);
                    await _context.SaveChangesAsync();
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
