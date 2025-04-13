using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static WMS_Application.Repositories.AdminsRepository;
using WMS_Application.DTO;
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
            var users = await _context.TblUsers
                .Where(x => x.RoleId == 2 &&
                           (x.VerificationStatus != "Pending" ||
                            _context.TblAdminInfos.Any(a => a.AdminId == x.UserId)) && (x.IsDeleted == false && x.IsActive == true))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return users;
        }
        public async Task<List<TblRole>> GetAllRoles()
        {
            return await _context.TblRoles.Where(x => x.RoleId > 2 && x.RoleId != 5 && x.IsActive == true).ToListAsync();
        }

        public List<AdminReportsDTO> GetAdminReports()
        {
            var adminData = _context.TblUsers
                .Where(x => x.RoleId == 2 && x.IsDeleted == false)
                .Select(data => new AdminReportsDTO
                {
                    AdminId = data.UserId,
                    FullName = (data.FirstName == null && data.LastName == null) ? data.Username : $"{data.FirstName} {data.LastName}",
                    Email = data.Email,
                    RegisteredOn = (DateTime)data.CreatedAt,
                    ShopDetails = _context.TblShops.Any(x => x.AdminId == data.UserId),
                    Documents = _context.TblAdminInfos.Any(x => x.AdminId == data.UserId),
                    Status = data.VerificationStatus,
                    Employees = _context.TblUsers.Count(x => x.AdminRef == data.UserId),
                    ProfilePic = data.ProfileImgPath,
                    IsActive = (bool)data.IsActive
                }).OrderByDescending(x => x.RegisteredOn)
                .ToList();
            return adminData;
        }

        public async Task<object> UpdateStatus(int userId, bool status, string verifier, string remark)
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
                ? $"Hi {user.Username},<br>Your account is approved.<br><b>Remark:</b> {remark}<br> Log in to start."
                : $"Hi {user.Username},<br>Your account is rejected. Please contact support. <br><b>Remark:</b> {remark}<br>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return new { success = true, message = status ? "User approved." : "User rejected." };
        }


        public TblUser checkExistence(string Username, string email, int UserId)
        {
            var existingUser = _context.TblUsers
            .Where(u => u.UserId != UserId) // Exclude the current admin being edited
            .FirstOrDefault(u => u.Email == email || u.Username == Username);

            return existingUser;
        }

        public async Task<object> SaveUsers(TblUser user)
        {
            try
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
                    Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + user.ProfileImage.FileName;

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        user.ProfileImage.CopyTo(stream);
                    }

                    imgPath = "\\UserUploads\\" + uniqueFileName;
                }
                if (user.UserId > 0)
                {

                    UpdatedUser.Username = user.Username;
                    UpdatedUser.FirstName = user.FirstName;
                    UpdatedUser.LastName = user.LastName;
                    UpdatedUser.PhoneNumber = user.PhoneNumber;
                    UpdatedUser.CreatedAt = user.CreatedAt;
                    UpdatedUser.RoleId = user.RoleId;
                    if (user.ProfileImage != null)
                    {
                        UpdatedUser.ProfileImgPath = imgPath;
                    }
                }
                if (user.PasswordHash != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }
                user.ProfileImage = null;
                if (user.ProfileImgPath == null)
                {
                    user.ProfileImgPath = imgPath;
                }
                user.VerificationStatus = "Approved";
                string msg = "";
                string type = "Admins";
                if (user.RoleId > 2)
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
                return new { success = false, message = "Unkown error occured" };
            }
        }
    }
}
