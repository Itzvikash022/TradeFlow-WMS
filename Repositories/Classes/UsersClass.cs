using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
namespace WMS_Application.Repositories.Classes
{
    public class UsersClass : UsersInterface
    {
    private readonly dbMain _context;
    public UsersClass(dbMain context)
    {
        _context = context;
    }

    public async Task<object> SaveUsers(User user)
    {
            if (user.ProfileImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(user.ProfileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed." };
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


            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return new { success = true, message = "User added successfully." };

        }

        public async Task<bool> IsUsernameExists(string Username)
    {
        return await _context.Users.AnyAsync(x => x.Username == Username);
    }       

    public async Task<bool> IsEmailExists(string Email)
    {
        return await _context.Users.AnyAsync(x => x.Email == Email);
    }

        public async Task<bool> IsAdminExists(string AdminRef)
        {
            return await _context.Users.AnyAsync(x => x.Username == AdminRef && x.Role == "Admin");
        }

        public async Task<List<Designation>> GetDesignations()
        {
            return await _context.Designations
           .Select(d => new Designation
           {
               Designation1 = d.Designation1
           })
           .ToListAsync();
        }
}
}
