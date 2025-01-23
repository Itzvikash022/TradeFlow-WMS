using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
namespace WMS_Application.Repositories.Auth
{
    public class UsersRepository : IUsersRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;

        public UsersRepository(dbMain context, IEmailSenderRepository emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<bool> IsUsernameExists(string Username)
        {
            return await _context.TblUsers.AnyAsync(x => x.Username == Username);
        }

        public async Task<bool> IsVerified(string cred)
        {
            var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == cred || x.Username == cred);
            return user.IsVerified;
        }

        public async Task<bool> hasShopDetails(int id)
        {
            return await _context.TblShops.AnyAsync(y => y.AdminId == id); ;
        }

        public async Task<TblUser> GetUserDataByEmail(string email)
        {
            return await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<TblShop> GetShopDataByUserId(int id)
        {
            return await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == id);
        }
        public async Task<TblAdminInfo> GetAdminDocDetailsById(int id)
        {
            return await _context.TblAdminInfos.FirstOrDefaultAsync(x => x.AdminId == id);
        }

        public async Task<bool> hasAdminDoc(int id)
        {
            return await _context.TblAdminInfos.AnyAsync(z => z.AdminId == id);
        }

        public async Task<string> fetchEmail(string cred)
        {
            return await _context.TblUsers
                .Where(u => u.Email == cred || u.Username == cred)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> IsEmailExists(string Email)
        {
            return await _context.TblUsers.AnyAsync(x => x.Email == Email);
        }

        //This method will send verification Otp to user's email and save userData into db
        public async Task<object> SaveUsers(TblUser user)
        {
            //Encrypting password using bcrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            //Generating OTP and sending it to user's email
            user.Otp = _emailSender.GenerateOtp();
            user.OtpExpiry = DateTime.Now.AddMinutes(5);
            user.IsVerified = false;
            string subj = "OTP Verification!!";
            await _emailSender.SendEmailAsync(user.Email, subj, user.Otp);
            user.RoleId = 2;
            //Saving all given data to db
            await _context.TblUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            return new { success = true, message = "Registered Successfully, OTP sent to email, please verify to move forward" };
        }

        //This method will check if the otp given by user is correct or not
        public async Task<bool> OtpVerification(string otp)
        {
            return await _context.TblUsers.AnyAsync(x => x.Otp == otp && x.OtpExpiry > DateTime.Now);
        }

        //This method will update the status of user to verified
        public async Task<object> updateStatus(string Email)
        {
            var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.Email == Email);
            if (user != null)
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
                return new { success = true, message = "Email verified successfully" };
            }
            return new { success = false, message = "Email not found" };
        }


        //Saving other data after OTP verification
        public async Task<object> SaveMoreDetails(TblUser user)
        {
            var UpdatedUser = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == user.Email);

            //Image Validation
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

                UpdatedUser.ProfileImgPath = "\\UserUploads\\" + uniqueFileName;
                user.ProfileImage = null;
            }

            if (UpdatedUser == null)
            {
                return new { success = false, message = "User not found." };
            }
            UpdatedUser.FirstName = user.FirstName;
            UpdatedUser.LastName = user.LastName;
            UpdatedUser.PhoneNumber = user.PhoneNumber;
            UpdatedUser.DateOfBirth = user.DateOfBirth;

            _context.TblUsers.Update(UpdatedUser);
            await _context.SaveChangesAsync();

            return new { success = true, message = "More details have beed saved successfully" };
        }

        public async Task<object> SaveShopDetails(TblShop shop, int id)
        {
            if (shop.ShopImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(shop.ShopImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ShopUploads\\Images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + shop.ShopImage.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    shop.ShopImage.CopyTo(stream);
                }

                shop.ShopImagePath = "\\ShopUploads\\Images\\" + uniqueFileName;
                shop.ShopImage = null;
            }

            shop.AdminId = id;
            await _context.TblShops.AddAsync(shop);
            await _context.SaveChangesAsync();

            return new { success = true, message = "Shop data added successfully enjoyy" };
        }

        public async Task<object> SaveAdminDoc(TblAdminInfo info, int id)
        {
            if (info.IdentityDoc != null)
            {
                var allowedExtensions = new[] { ".pdf" };
                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;
                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(info.IdentityDoc.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .pdf is allowed." };
                }
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ShopUploads\\Documents\\IdentityDoc");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + info.IdentityDoc.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    info.IdentityDoc.CopyTo(stream);
                }
                info.IdentityDocPath = "\\ShopUploads\\Documents\\IdentityDoc\\" + uniqueFileName;
                info.IdentityDoc = null;
            }
            if (info.AddressProof != null)
            {
                var allowedExtensions = new[] { ".pdf" };
                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;
                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(info.AddressProof.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .pdf is allowed." };
                }
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ShopUploads\\Documents\\AddressDoc");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + info.AddressProof.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    info.AddressProof.CopyTo(stream);
                }
                info.AddressProofPath = "\\ShopUploads\\Documents\\AddressDoc\\" + uniqueFileName;
                info.AddressProof = null;
            }
            if (info.ShopLicense != null)
            {
                var allowedExtensions = new[] { ".pdf" };
                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;
                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(info.ShopLicense.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .pdf is allowed." };
                }
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ShopUploads\\Documents\\LicenseDoc");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + info.ShopLicense.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    info.ShopLicense.CopyTo(stream);
                }
                info.ShopLicensePath = "\\ShopUploads\\Documents\\LicenseDoc\\" + uniqueFileName;
                info.ShopLicense = null;
            }
            info.VerificationStatus = "Pending";
            info.AdminId = id;
            await _context.TblAdminInfos.AddAsync(info);
            await _context.SaveChangesAsync();
            return new { success = true, message = "Documents added successfully" };
            }
        }
}
