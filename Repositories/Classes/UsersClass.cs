using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
namespace WMS_Application.Repositories.Classes
{
    public class UsersClass : UsersInterface
    {
    private readonly dbMain _context;
    private readonly EmailSenderInterface _emailSender;

        public UsersClass(dbMain context, EmailSenderInterface emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

    public async Task<object> SaveUsers(User user)
    {
            if (user.ProfileImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(user.ProfileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + user.ProfileImage.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    user.ProfileImage.CopyTo(stream);
                }

                // Hash the password

                user.ProfileImgPath = "\\Uploads\\" + uniqueFileName;
                user.ProfileImage = null;
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            user.Otp=  _emailSender.GenerateOtp();
            user.OtpExpiry = DateTime.Now.AddMinutes(5);
            string subj = "OTP Verification!!";
            await _emailSender.SendEmailAsync(user.Email,subj, user.Otp);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return new { success = true, message = "Registered Successfully, OTP sent to email, please verify to move forward" };

        }

        public async Task<bool> OtpVerification(string otp)
        {
            return await _context.Users.AnyAsync(x => x.Otp == otp && x.OtpExpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == Email);
            if (user != null)
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
                return new { success = true, message = "Email verified successfully" };
            }
            return new { success = false, message = "Email not found" };
        }

        public async Task<bool> IsUsernameExists(string Username)
    {
        return await _context.Users.AnyAsync(x => x.Username == Username);
    }       

    public async Task<bool> IsEmailExists(string Email)
    {
        return await _context.Users.AnyAsync(x => x.Email == Email);
    }

        //public async Task<bool> IsAdminExists(string AdminRef)
        //{
        //    return await _context.Users.AnyAsync(x => x.Username == AdminRef && x.Role == "Admin");
        //}

        public async Task<List<Designation>> GetDesignations()
        {
            return await _context.Designations.ToListAsync();
        }

        public async Task<List<string>> GetAdminUsernames()
        {
            return await _context.Users
            .Where(u => u.Role == "Admin")
            .Select(u => u.Username)
            .ToListAsync();
        }
    }
}
