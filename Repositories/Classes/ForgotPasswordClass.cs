using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Policy;
using WMS_Application.Models;
using Microsoft.AspNetCore.Mvc;

using WMS_Application.Repositories.Interfaces;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
namespace WMS_Application.Repositories.Classes
{
    public class ForgotPasswordClass : ForgotPasswordInterface
    {
        private readonly dbMain _context;
        private readonly EmailSenderInterface _emailSender;

        private readonly IMemoryCache _memoryCache;

        public ForgotPasswordClass(dbMain context, EmailSenderInterface emailSender, IMemoryCache memoryCache)
        {
            _context = context;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }
        public async Task<object> TokenSenderViaEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new { success = false, message = "No user found with that email or username" };
            }

            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddHours(1); // Token expires in 1 hour
            
            // Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { UserId = user.UserId, ExpirationTime = expirationTime }, expirationTime);

            // Create the password reset URL with the token
            var resetUrl = "http://localhost:5026/Home/ResetPassword?token=" + resetToken;


            // Send the reset email
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Request",
                $"Click <a href='{resetUrl}'>here</a> to reset your password.");

            return new { success = true, message = "Password reset link sent to your email." };
        }

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
