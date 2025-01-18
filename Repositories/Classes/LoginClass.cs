using Microsoft.EntityFrameworkCore;
using System;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
namespace WMS_Application.Repositories.Classes
{
    public class LoginClass : LoginInterface
    {
        private readonly dbMain _context;

        public LoginClass(dbMain context)
        {
            _context = context;
        }

        public async Task<object> AuthenticateUser(string emailOrUsername, string password)
        {
            // Fetch user by email or username
            var user = await _context.Users
           .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                return new { success = false, message = "User not found" };
            }

            // Compare hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            {
                return new { success = true, message = "You are successfully logged in", email = user.Email };
            }
            return new { success = false, message = "Invalid user id or pass" };

        }
    }
}
