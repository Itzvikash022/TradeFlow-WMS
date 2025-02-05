using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
namespace WMS_Application.Repositories
{
    public class AdminsRepository : IAdminsRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;

        public AdminsRepository(dbMain context, IEmailSenderRepository emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<List<TblUser>> GetAllAdminsData()
        {
            var user = _context.TblUsers.Where(x => x.RoleId == 2).ToList();
            return user;
        }

        public async Task<List<TblRole>> GetAllRoles()
        {
            return await _context.TblRoles.ToListAsync();
        }

        public async Task<object> UpdateStatus(int userId, bool status, string verifier)
        {
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return new { success = false, message = "User not found." };

            user.VerificationStatus = status ? "Approved" : "Rejected";
            user.VerifiedBy = verifier;
            _context.TblUsers.Update(user);
            await _context.SaveChangesAsync();

            var userData = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId);
            var subject = status ? "Account Approved" : "Account Rejected";
            var body = status
                ? $"Hi {user.Username},<br>Your account is approved. Log in to start."
                : $"Hi {user.Username},<br>Your account is rejected. Please contact support.";
            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return new { success = true, message = status ? "User approved." : "User rejected." };
        }


        public TblUser checkExistence(string Username, string email,  int UserId)
        {
            var existingUser = _context.TblUsers
            .Where(u => u.UserId != UserId) // Exclude the current admin being edited
            .FirstOrDefault(u => u.Email == email || u.Username == Username);

            return existingUser;
        }

        public async Task<object> SaveUsers(TblUser user)
        {
            var UpdatedUser = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            string imgPath = "";
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

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\UserUploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + user.ProfileImage.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    user.ProfileImage.CopyTo(stream);
                }

                imgPath = "\\UserUploads\\" + uniqueFileName;
            }
            if(user.UserId > 0)
            {
                
                UpdatedUser.Username = user.Username;
                UpdatedUser.FirstName = user.FirstName;
                UpdatedUser.LastName = user.LastName;
                UpdatedUser.PhoneNumber = user.PhoneNumber;
                UpdatedUser.CreatedAt = user.CreatedAt;
                if(user.ProfileImage != null)
                {
                    UpdatedUser.ProfileImgPath = imgPath;
                }
            }
            if (user.PasswordHash != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }
            user.ProfileImage = null;
            user.ProfileImgPath = imgPath;
            user.VerificationStatus = "Approved";
            string msg = "";
            string type = "Admins";
            if(user.RoleId > 2)
            {
                type = "Employees";
            }
            if (user.UserId > 0)
            {
                _context.TblUsers.Update(UpdatedUser);
                msg = $"{type} data has been updated successfully";
            }
            else
            {
                _context.TblUsers.AddAsync(user);
                msg = $"New {type} has been added successfully";
            }
            await _context.SaveChangesAsync();

            return new { success = true, message = msg, role = type };
        }
    }
}
