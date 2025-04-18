using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WMS_Application.Models;
using WMS_Application.DTO;
using WMS_Application.Repositories.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DocumentFormat.OpenXml.EMMA;
using NuGet.Protocol.Plugins;


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
        private readonly ICompanyRepository _company;
        private readonly HttpClient _httpClient;
        private readonly IActivityRepository _activity;


        public AuthController(ILogger<AuthController> logger, dbMain context, IUsersRepository users, IEmailSenderRepository emailSender, ILoginRepository login, IMemoryCache memoryCache, ICompanyRepository company, IHttpClientFactory httpClientFactory, IActivityRepository activity)
        {
            _logger = logger;
            _context = context;
            _users = users;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
            _company = company;
            _httpClient = httpClientFactory.CreateClient();
            _activity = activity;
        }

        public IActionResult Index()
        {
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
        public IActionResult GoogleDetails(string email)
        {
            var model = new GoogleSignupModel { Email = email };
            return View(model);
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("auth/google-callback-view")]
        public IActionResult GoogleCallback()
        {
            return View("GoogleCallback", new object()); // This is your Razor view with JS
        }

        [HttpGet("auth/google-callback")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return Json(new { success = false, message = "Google authentication failed. Please try again." });
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity.Name;

            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && user.IsGoogleAccount == false)
            {
                return Json(new { success = false, message = "This email is already registered manually. Use Email/Password to login." });
            }

            if (user != null && user.IsGoogleAccount == true)
            {
                var info = await _context.TblAdminInfos.FirstOrDefaultAsync(u => u.AdminId == user.UserId);
                TblShop shopData = new TblShop();

                if (user.RoleId <= 2)
                    shopData = _context.TblShops.FirstOrDefault(x => x.AdminId == user.UserId);
                else
                    shopData = _context.TblShops.FirstOrDefault(x => x.AdminId == user.AdminRef);

                if (shopData != null)
                    HttpContext.Session.SetInt32("ShopId", shopData.ShopId);

                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetInt32("UserRoleId", user.RoleId);
                HttpContext.Session.SetString("UserEmail", email);


                // Check conditions and return messages accordingly
                if (user.RoleId == 2)
                {
                    if (!await _users.hasShopDetails(user.UserId))
                    {
                        TempData["shopleft-toast"] = "Shop details are missing. Please complete you shop details.";
                        TempData["shopleft-toastType"] = "error";
                        return Json(new { success = false, message = "Shop details are missing. Please complete you shop details.", redirect = Url.Action("ShopDetails", "Auth") });
                    }

                    if (!await _users.hasAdminDoc(user.UserId))
                    {
                        TempData["docleft-toast"] = "Verification Documents are not uploaded yet.";
                        TempData["docleft-toastType"] = "error";
                        return Json(new { success = false, message = "Documents are not uploaded yet.", redirect = Url.Action("AdminDoc", "Auth") });
                    }
                }

                if (info != null)
                {
                    if (user.VerificationStatus == "Rejected")
                        return Json(new { success = false, message = "Your account is rejected. Contact support." });

                    if (user.VerificationStatus == "Pending")
                        return Json(new { success = false, message = "Your account is pending verification." });
                }

                user.LastLogin = DateTime.Now;
                _context.TblUsers.Update(user);
                _context.SaveChanges();
                return Json(new { success = true, message = "Login successful!", redirect = Url.Action("", "Dashboard") });
            }

            // New Google user
            return Json(new
            {
                success = true,
                message = "Google account not found. Please complete your details.",
                redirect = Url.Action("GoogleDetails", "Auth", new { email })
            });
        }





        [HttpPost]
        public async Task<IActionResult> GoogleDetails(GoogleSignupModel model)
        {
            // Check if username already exists
            bool usernameExists = _context.TblUsers.Any(u => u.Username == model.Username);
            if (usernameExists)
            {
                return Json(new { success = false, message = "Username Already Exists" });
            }

            // Create temp password
            string tempPassword = Guid.NewGuid().ToString();

            // Create user
            var user = new TblUser
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                IsGoogleAccount = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = 2,
                VerificationStatus = "Pending"
            };

            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("UserRoleId", user.RoleId);


            _context.TblUsers.Add(user);
            await _context.SaveChangesAsync();
            TempData["google-toast"] = "Account Created Successfully using Google!";
            TempData["google-toastType"] = "success";

            return Json(new { success = true, message = "Account Created Successfully" });
        }

        [HttpGet("/Auth/states")]
        public async Task<IActionResult> GetStates()
        {
            var response = await _httpClient.GetStringAsync("http://api.geonames.org/childrenJSON?geonameId=1269750&username=itzvikash");
            return Content(response, "application/json");
        }

        [HttpGet("/Auth/cities/{geonameId}")]
        public async Task<IActionResult> GetCities(int geonameId)
        {
            var url = $"http://api.geonames.org/childrenJSON?geonameId={geonameId}&username=itzvikash";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
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

            TempData["reset-toast"] = "Password Changed Successgfully!";
            TempData["reset-toastType"] = "success";
            return Ok(res);
        }
        public IActionResult OtpCheck()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult CompanyRegistration()
        {
            return View(new TblCompany());
        }
        public IActionResult CompanyLogin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CompanyLogin([FromForm] string email, string passwordhash)
        {
            var result = await _company.AuthenticateUser(email, passwordhash);
            var companyData = _context.TblCompanies.FirstOrDefault(x => x.Email == email);
            if (((dynamic)result).success)
            {
                HttpContext.Session.SetInt32("UserRoleId", 5);
                HttpContext.Session.SetInt32("CompanyId", companyData.CompanyId);
                companyData.LastLogin = DateTime.Now;
                _context.TblCompanies.Update(companyData);
                _context.SaveChanges();
            }
            TempData["toast"] = "Login successful!";
            TempData["toastType"] = "success";
            return (Json(result));
        }

        [HttpPost]
        public async Task<IActionResult> CompanyRegistration([FromForm] TblCompany company)
        {
            try
            {
                int compId = company.CompanyId;

                if (await _company.IsEmailExists(company.Email) && company.CompanyId == 0)
                {
                    return Json(new { success = false, message = "Email already exists" });
                }
                HttpContext.Session.SetString("UserEmail", company.Email);
                TempData["companyreg-toast"] = "Company Saved Successfully";
                TempData["companyreg-toastType"] = "success";

                var result = await _company.SaveCompany(company);
                if (((dynamic)result).success)
                {
                    int id = 0;
                    string type = "", desc = "";

                    if (company.Path == "MyCompany")
                    {
                        id = (int)HttpContext.Session.GetInt32("CompanyId");
                        type = "Self Company Details Update";
                        desc = $"{company.CompanyName} Updated his Company Details";
                    }
                    else if (company.Path == "Company")
                    {
                        id = (int)HttpContext.Session.GetInt32("UserId");
                        string username = _context.TblUsers.Where(x => x.UserId == id).Select(s => s.Username).FirstOrDefault();
                        type = "Company Details Update";
                        if (compId > 0)
                        {
                            desc = $"{username} updated {company.CompanyName}'s Company Details";
                        }
                        else
                        {
                            desc = $"{username} added a new Company named : {company.CompanyName}";
                        }
                    }


                    _activity.AddNewActivity(id, (int)HttpContext.Session.GetInt32("UserRoleId"), type, desc);
                }
                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });
            }
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

                TblShop shopData = new TblShop();
                if (data.RoleId <= 2)
                {
                    shopData = _context.TblShops.FirstOrDefault(x => x.AdminId == id);
                }
                else
                {
                    int refId = data.AdminRef;
                    shopData = _context.TblShops.FirstOrDefault(x => x.AdminId == refId);
                }
                if (shopData != null)
                {
                    HttpContext.Session.SetInt32("ShopId", shopData.ShopId);
                }
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetInt32("UserRoleId", data.RoleId);

                // Determine redirection based on user verification and shop details
                if (data.RoleId != 1)
                {
                    if (await _users.IsVerified(login.EmailOrUsername))
                    {
                        if (data.RoleId == 2)
                        {
                            if (await _users.hasShopDetails(id))
                            {
                                if (!await _users.hasAdminDoc(id))
                                {
                                    TempData["doc-toast"] = "Complete you Documentation form";
                                    TempData["doc-toastType"] = "warning";
                                    RedirectTo = "AdminDoc";
                                }
                            }
                            else
                            {
                                TempData["shop-toast"] = "Complete you shop form";
                                TempData["shop-toastType"] = "warning";
                                RedirectTo = "ShopDetails"; // Redirect to shop details if not filled
                            }
                        }
                    }
                    else
                    {
                        RedirectTo = "OtpCheck"; // Redirect to OTP verification if not verified
                    }
                }

                if (RedirectTo == "OtpCheck")
                {
                    var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == email);
                    if (user != null)
                    {
                        user.Otp = _emailSender.GenerateOtp();
                        user.OtpExpiry = DateTime.Now.AddMinutes(5);

                        string emailBody = @"
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset='UTF-8'>
                                    <title>OTP Verification</title>
                                </head>
                                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                                    <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                        <h2 style='color: #333;'>Verify Your Email</h2>
                                        <p style='font-size: 16px; color: #555;'>Your One-Time Password (OTP) for email verification is:</p>
                                        <div style='font-size: 24px; font-weight: bold; color: #2c3e50; margin: 20px 0; text-align: center;'>
                                            " + user.Otp + @"
                                        </div>
                                        <p style='font-size: 14px; color: #888;'>This OTP is valid for a limited time. Please do not share it with anyone.</p>
                                        <p style='margin-top: 30px; font-size: 12px; color: #aaa;'>If you did not request this email, you can safely ignore it.</p>
                                    </div>
                                </body>
                                </html>";


                        await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", emailBody);
                        await _context.SaveChangesAsync();

                        TempData["otp-toast"] = "OTP sent Successfully";
                        TempData["otp-toastType"] = "success";
                    }

                }

                // Reset login attempts after successful login
                _memoryCache.Remove(attemptKey);
                TempData["toast"] = "Login successful!";
                TempData["toastType"] = "success";


                data.LastLogin = DateTime.Now;
                _context.TblUsers.Update(data);
                _context.SaveChanges();
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

            TempData["moredetails-toast"] = "Details Saved Successfully";
            TempData["moredetails-toastType"] = "success";

            var result = await _users.SaveMoreDetails(user);
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string type = "Update My Profile";
            string desc = $"{user.Username} updated his profile";
            _activity.AddNewActivity(id, 2, type, desc);

            return Json(result);
        }
        public async Task<IActionResult> ShopDetails()
        {
            var model = new TblShop();
            int id = (int)HttpContext.Session.GetInt32("UserId");
            if (id != 0)
            {
                model = await _users.GetShopDataByUserId(id) ?? new TblShop();
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
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.oldPassword) || string.IsNullOrWhiteSpace(model.newPassword))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            int userId = HttpContext.Session.GetInt32("UserId").Value; // Get logged-in user ID
            var user = await _context.TblUsers.FindAsync(userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.oldPassword, user.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Old password is incorrect." });
            }

            if (model.newPassword.Length < 8)
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.newPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> AdminDoc(TblAdminInfo info)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            if (id != null)
            {

                string subject = "";
                string body = "";
                string email = HttpContext.Session.GetString("UserEmail");
                var user = await _users.GetUserDataByEmail(email);
                string adminEmail = "vikash.my022@gmail.com";
                string resetUrl = "https://localhost:5026/Admins";

                if (user.VerificationStatus != "Pending")
                {
                    subject = "Admin has logged for first time";
                    body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <title>Admin First Login</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                            <div style='max-width: 600px; margin: auto; background-color: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                <h2 style='color: #333;'>Admin Logged In</h2>
                                <p><strong>{user.Username}</strong> has logged in for the first time and completed their documentation and shop details.</p>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p>Click the button below to go to the Admin Panel:</p>
                                <p style='margin: 25px 0;'>
                                    <a href='{resetUrl}' style='background-color: #4CAF50; color: white; padding: 12px 20px; border-radius: 5px; text-decoration: none;'>Go to Admin Panel</a>
                                </p>
                            </div>
                        </body>
                        </html>";
                    
                }
                else
                {
                    subject = "New User Registration - Pending Approval";
                    body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <title>New User Registration</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                            <div style='max-width: 600px; margin: auto; background-color: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                <h2 style='color: #333;'>New User Registration</h2>
                                <p>A new user has registered and is awaiting approval:</p>
                                <p><strong>Username:</strong> {user.Username}<br>
                                   <strong>Email:</strong> {user.Email}<br>
                                   <strong>Date:</strong> {DateTime.UtcNow}</p>
                                <p>Click the button below to review in the Admin Panel:</p>
                                <p style='margin: 25px 0;'>
                                    <a href='{resetUrl}' style='background-color: #007BFF; color: white; padding: 12px 20px; border-radius: 5px; text-decoration: none;'>Review Now</a>
                                </p>
                            </div>
                        </body>
                        </html>";

                }
                await _emailSender.SendEmailAsync(adminEmail, subject, body);

                TempData["admindoc-toast"] = "Document details Saved Successfully";
                TempData["admindoc-toastType"] = "success";
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
            int id = (int)HttpContext.Session.GetInt32("UserId");
            shop.IsActive = true;
            if (shop.IsAction == "SPAddNew" || shop.IsAction == "SPUpdate")
            {
                TempData["shopdetails-toast"] = "Shop details Saved Successfully";
                TempData["shopdetails-toastType"] = "success";
                var result = await _users.SaveShopDetails(shop, shop.AdminId);


                if (((dynamic)result).success)
                {
                    string username = _context.TblUsers.Where(x => x.UserId == id).Select(x => x.Username).FirstOrDefault();
                    string shopKeeperName = _context.TblUsers.Where(x => x.UserId == shop.AdminId).Select(x => x.Username).FirstOrDefault();
                    string type, desc;
                    if (shop.IsAction == "SPAddNew")
                    {
                        type = "Shop Details Add";
                        desc = $"{username} added {shopKeeperName}'s Shop Details";
                    }
                    else
                    {
                        type = "Shop Details Update";
                        desc = $"{username} updated {shopKeeperName}'s Shop Details";
                    }

                    _activity.AddNewActivity(id, (int)HttpContext.Session.GetInt32("UserRoleId"), type, desc);
                    //TblActivityLog activity = new TblActivityLog()
                    //{
                    //    UserId = id,
                    //    Role = (int)HttpContext.Session.GetInt32("UserRoleId"),
                    //    ActivityType = type,
                    //    Description = desc
                    //};
                    //_context.TblActivityLogs.Add(activity);
                    //_context.SaveChanges();
                }

                return Json(result);
            }
            else
            {
                if (id != null)
                {
                    TempData["shopdetails-toast"] = "Shop details Saved Successfully";
                    TempData["shopdetails-toastType"] = "success";

                    var result = await _users.SaveShopDetails(shop, id);

                    if (((dynamic)result).success)
                    {
                        string username = _context.TblUsers.Where(x => x.UserId == id).Select(x => x.Username).FirstOrDefault();
                        TblActivityLog activity = new TblActivityLog()
                        {
                            UserId = id,
                            Role = (int)HttpContext.Session.GetInt32("UserRoleId"),
                            ActivityType = "Self Shop Details Update",
                            Description = $"{username} updated his Shop Details"
                        };
                        _context.TblActivityLogs.Add(activity);
                        _context.SaveChanges();
                    }
                    return Json(result);
                }
                else
                {
                    return Json(new { success = false, message = "Owner Id not found" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> OtpCheck(TblUser user)
        {
            try
            {
                string email = HttpContext.Session.GetString("UserEmail");
                if (await _users.IsEmailExists(email))
                {
                    if (await _users.OtpVerification(user.Otp))
                    {
                        await _users.updateStatus(email);
                        int roleId = _context.TblUsers.Where(x => x.Email == email).Select(y => y.RoleId).FirstOrDefault();
                        //int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                        if (roleId > 2)
                        {
                            return Json(new { success = true, message = "OTP verified successfully", emp = true });
                        }
                        //_emailSender.SendEmailAsync()
                        TempData["otpcheck-toast"] = "OTP Verified Successfully";
                        TempData["otpcheck-toastType"] = "success";
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

                TempData["register-toast"] = "Registered Successfully, OTP sent to email, please verify to move forward";
                TempData["register-toastType"] = "success";
                HttpContext.Session.SetInt32("UserRoleId", 2);
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

                    string emailBody = @"
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset='UTF-8'>
                                    <title>OTP Verification</title>
                                </head>
                                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                                    <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                        <h2 style='color: #333;'>Verify Your Email</h2>
                                        <p style='font-size: 16px; color: #555;'>Your One-Time Password (OTP) for email verification is:</p>
                                        <div style='font-size: 24px; font-weight: bold; color: #2c3e50; margin: 20px 0; text-align: center;'>
                                            " + user.Otp + @"
                                        </div>
                                        <p style='font-size: 14px; color: #888;'>This OTP is valid for a limited time. Please do not share it with anyone.</p>
                                        <p style='margin-top: 30px; font-size: 12px; color: #aaa;'>If you did not request this email, you can safely ignore it.</p>
                                    </div>
                                </body>
                                </html>";

                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", emailBody);
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
