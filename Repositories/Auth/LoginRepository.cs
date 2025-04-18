using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
namespace WMS_Application.Repositories.Auth
{
    public class LoginRepository : ILoginRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IMemoryCache _memoryCache;

        public LoginRepository(dbMain context, IEmailSenderRepository emailSender, IMemoryCache memoryCache)
        {
            _context = context;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        //Here we are authenticating user by first checking user by email or username and then comparing hashed passwords
        public async Task<object> AuthenticateUser(string emailOrUsername, string password)
        {
            const int maxAttempts = 5; // Maximum allowed attempts
            const int lockoutDurationSeconds = 300; // Lockout duration in seconds

            // Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{emailOrUsername}";
            var lockoutKey = $"Lockout_{emailOrUsername}";

            //Fetching user by email or username
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                return new { success = false, message = "User not found" };
            }

            if (user != null && user.IsGoogleAccount == true)
            {
                return new { success = false, message = "This account is linked with Google. Please use Google login." };
            }


            var info = await _context.TblAdminInfos
           .FirstOrDefaultAsync(u => u.AdminId == user.UserId);
            //Comparing hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            {
                if(user.RoleId != 1)
                {
                    if(info != null)
                    {
                        if (user.VerificationStatus == "Rejected")
                        {
                            return new { success = false, message = "You're rejected by the SuperAdmins, please contact them for more information." };
                        }
                        if (user.VerificationStatus == "Pending")
                        {
                            return new { success = false, message = "You're yet to be verified by the SuperAdmins, please wait for awhile." };
                        }
                    }
                }
                _memoryCache.Remove(attemptKey);
                if (user.IsDeleted == true)
                {
                    return new { success = false, message = "You account has been deleted" };
                }
                if (user.IsActive == false)
                {
                    return new { success = false, message = "Your account has been restricted" };
                }
                return new { success = true, message = "You are successfully logged in"};
            }
           
            // Check if the user is locked out
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." };
            }

         int attempts = _memoryCache.GetOrCreate(attemptKey, entry =>
         {
             entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lockoutDurationSeconds);
             return 0;
         });

            attempts++;
            _memoryCache.Set(attemptKey, attempts, TimeSpan.FromSeconds(lockoutDurationSeconds));

            // Lockout the user if max attempts reached
            if (attempts >= maxAttempts)
            {
                _memoryCache.Set(lockoutKey, DateTime.Now.AddSeconds(lockoutDurationSeconds), TimeSpan.FromSeconds(lockoutDurationSeconds));
                _memoryCache.Remove(attemptKey); // Reset attempts after lockout
                return new { success = false, message = $"Account is locked. Try again in {lockoutDurationSeconds} seconds." };
            }

            return new { success = false, message = $"Invalid credentials. You have {maxAttempts - attempts} attempts left." };
        }

        //This method will send token to user's email for password reset
        public async Task<object> TokenSenderViaEmail(string email, string isCompany)
        {
            int Id = 0;
            var resetUrl = "";
            if (isCompany == "true")
            {
                var company = await _context.TblCompanies.FirstOrDefaultAsync(u => u.Email == email);
                if (company == null)
                {
                    return new { success = false, message = "No company found with that email or username" };
                }
                Id = company.CompanyId;
            }
            else
            {
                //Fetching user by email
                var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return new { success = false, message = "No user found with that email or username" };
                }
                Id = user.UserId;
            }

            //We are creating token variables here
            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddHours(1); // Token expires in 1 hour

            // Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { UserId = Id, ExpirationTime = expirationTime }, expirationTime);

            // Create the password reset URL with the token

            if(isCompany == "true")
            {
                resetUrl = "http://localhost:5026/Auth/ResetCompanyPassword?token=" + resetToken;
            }
            else
            {
                resetUrl = "http://localhost:5026/Auth/ResetPassword?token=" + resetToken;
            }

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            width: 100%;
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 10px;
                            box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            text-align: center;
                            padding-bottom: 20px;
                        }}
                        .header h1 {{
                            color: #333333;
                            font-size: 24px;
                        }}
                        .content {{
                            font-size: 16px;
                            color: #555555;
                            line-height: 1.6;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background-color: #007bff;
                            color: #ffffff;
                            text-decoration: none;
                            font-size: 16px;
                            border-radius: 5px;
                            margin-top: 20px;
                        }}
                        .footer {{
                            text-align: center;
                            font-size: 14px;
                            color: #777777;
                            margin-top: 40px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <p>Hi there,</p>
                            <p>We received a request to reset your password. If you didn’t request a password reset, you can ignore this email.</p>
                            <p>To reset your password, click the button below:</p>
                            <a href='{resetUrl}' class='button'>Reset Password</a>
                            <p>If you have any issues, feel free to reach out to our support team.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} Your Company Name. All Rights Reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
            // Send the reset email
            await _emailSender.SendEmailAsync(email, "Password Reset Request",body);

            return new { success = true, message = "Password reset link sent to your email." };
        }

        //This method will reset password of user
        public async Task<object> ResetPassword(string creds, string newPassword, bool isCompany)
        {
            //_memoryCache.Remove(token);

            if (isCompany)
            {
                var company = _context.TblCompanies.FirstOrDefault(u => u.Email == creds);
                if (company != null)
                {
                    company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    _context.SaveChanges();
                    return new { success = true, message = "Password Changed Successfully" };
                }
            }
            else
            {
                var user = _context.TblUsers.FirstOrDefault(u => u.Email == creds || u.Username == creds);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    _context.SaveChanges();
                    return new { success = true, message = "Password Changed Successfully" };
                }
            }

            return new { success = false, message = "User not found" };
        }
    }
}
