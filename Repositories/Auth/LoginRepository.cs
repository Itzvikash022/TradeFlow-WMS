using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
namespace WMS_Application.Repositories.Auth
{
    public class LoginRepository : ILogin
    {
        private readonly dbMain _context;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;

        public LoginRepository(dbMain context, IEmailSender emailSender, IMemoryCache memoryCache)
        {
            _context = context;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        //Here we are authenticating user by first checking user by email or username and then comparing hashed passwords
        public async Task<object> AuthenticateUser(string emailOrUsername, string password)
        {
            //Fetching user by email or username
            var user = await _context.Users
           .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                return new { success = false, message = "User not found" };
            }

            //Comparing hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            {
                return new { success = true, message = "You are successfully logged in", email = user.Email };
            }

            return new { success = false, message = "Invalid user id or pass" };
        }

        //This method will send token to user's email for password reset
        public async Task<object> TokenSenderViaEmail(string email)
        {
            //Fetching user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

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

            var user = _context.Users.FirstOrDefault(u => u.Email == creds || u.Username == creds);
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
