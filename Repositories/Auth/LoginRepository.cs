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
        public async Task<object> TokenSenderViaEmail(string email)
        {
            //Fetching user by email
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new { success = false, message = "No user found with that email or username" };
            }

            //We are creating token variables here
            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddHours(1); // Token expires in 1 hour

            // Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { UserId = user.UserId, ExpirationTime = expirationTime }, expirationTime);

            // Create the password reset URL with the token
            var resetUrl = "http://localhost:5026/Auth/ResetPassword?token=" + resetToken;

            // Send the reset email
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Request",
                $"Click <a href='{resetUrl}'>here</a> to reset your password.");

            return new { success = true, message = "Password reset link sent to your email." };
        }

        //This method will reset password of user
        public async Task<object> ResetPassword(string creds, string newPassword)
        {
            //_memoryCache.Remove(token);

            var user = _context.TblUsers.FirstOrDefault(u => u.Email == creds || u.Username == creds);
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.SaveChanges();
                return new { success = true, message = "Password Changed Successfully" };

            }

            return new { success = false, message = "User not found" };
        }
    }
}
