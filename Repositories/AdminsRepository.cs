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
            var query = from admin in _context.TblUsers
                        join adminInfo in _context.TblAdminInfos on admin.UserId equals adminInfo.AdminId
                        where admin.RoleId == 2 // Filter by RoleId = 2
                        select new TblUser
                        {
                            UserId = admin.UserId,
                            Username = admin.Username,
                            FirstName = admin.FirstName,
                            LastName = admin.LastName,
                            Email = admin.Email,
                            RoleId = admin.RoleId,
                            PhoneNumber = admin.PhoneNumber,
                            CreatedAt = admin.CreatedAt,
                            ProfileImgPath = admin.ProfileImgPath,
                            VerificationStatus = adminInfo.VerificationStatus
                        };
            return await query.ToListAsync();
        }

        public async Task<object> UpdateStatus(int adminId, bool status)
        {
            var admin = await _context.TblAdminInfos.FirstOrDefaultAsync(u => u.AdminId == adminId);
            if (admin == null) return new { success = false, message = "User not found." };

            admin.VerificationStatus = status ? "Approved" : "Rejected";
            _context.TblAdminInfos.Update(admin);
            await _context.SaveChangesAsync();

            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == adminId);
            var subject = status ? "Account Approved" : "Account Rejected";
            var body = status
                ? $"Hi {user.Username},<br>Your account is approved. Log in to start."
                : $"Hi {user.Username},<br>Your account is rejected. Please contact support.";
            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return new { success = true, message = status ? "User approved." : "User rejected." };
        }
    }
}
