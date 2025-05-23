﻿using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Http;
namespace WMS_Application.Repositories.Auth
{
    public class UsersRepository : IUsersRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersRepository(dbMain context, IEmailSenderRepository emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
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

            string emailBody = @"
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset='UTF-8'>
                                    <title>OTP Verification</title>
                                </head>
                                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
                                    <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.05);'>
                                        <h2 style='color: #333;'>Verify Your Email</h2>
                                        <p style='font-size: 16px; color: #555;'>Your One-Time Password (OTP) for email verification is:</p>
                                        <div style='font-size: 24px; font-weight: bold; color: #2c3e50; margin: 20px 0; text-align: center;'>
                                            " + user.Otp + @"
                                        </div>
                                        <p style='font-size: 14px; color: #888;'>This OTP is valid for a limited time. Please do not share it with anyone.</p>
                                        <p style='margin-top: 30px; font-size: 12px; color: #aaa;'>If you did not request this email, you can safely ignore it.</p>
                                    </div>
                                </body>
                                </html>";


            await _emailSender.SendEmailAsync(user.Email, subj, emailBody);
            user.RoleId = 2;

            user.   VerificationStatus = "Pending";
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
                Directory.CreateDirectory(uploadsFolder);

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

            if(user.Head == "MyProfile")
            {
                return new { success = true, message = "Profile Updated Successfully", path=user.Head };
            }

            return new { success = true, message = "More details have beed saved successfully" };
        }
        public async Task<object> SaveShopDetails(TblShop shop, int id)
        {
            var UpdatedShopDetails =await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == id); 
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
                Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + shop.ShopImage.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    shop.ShopImage.CopyTo(stream);
                }

                    shop.ShopImagePath = "\\ShopUploads\\Images\\" + uniqueFileName;
            }

            if(UpdatedShopDetails != null)
            {
                UpdatedShopDetails.ShopName = shop.ShopName;
                UpdatedShopDetails.AdminId = id;
                UpdatedShopDetails.State = shop.State;
                UpdatedShopDetails.City = shop.City;
                UpdatedShopDetails.Pincode = shop.Pincode;
                UpdatedShopDetails.Address = shop.Address;
                UpdatedShopDetails.StartTime = shop.StartTime;
                UpdatedShopDetails.ClosingTime = shop.ClosingTime;
                if(shop.ShopId == _httpContextAccessor.HttpContext.Session.GetInt32("ShopId"))
                {
                    UpdatedShopDetails.MarginPercentage = shop.MarginPercentage;
                }
                if(shop.ShopImage != null)
                {
                    UpdatedShopDetails.ShopImagePath = shop.ShopImagePath;
                }
                _context.TblShops.Update(UpdatedShopDetails);
            }
            else
            {
                shop.AdminId = id;
                await _context.TblShops.AddAsync(shop);
            }
            shop.ShopImage = null;

            await _context.SaveChangesAsync();
            if(shop.IsAction == "UpdateSelf")
            {
                return new { success = true, message = "Shop data updated successfully", path="MyShop" };
            }
            else if (shop.IsAction == "SPAddNew")
            {
                return new { success = true, message = "Shop data added successfully", path = "Shops" };
            }
            else if (shop.IsAction == "SPUpdate")
            {
                return new { success = true, message = "Shop data updated successfully", path = "Shops" };
            }
            else
            {
                return new { success = true, message = "Shop data added successfully enjoyy" };
            }

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
                Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

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
                Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

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
                Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + info.ShopLicense.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    info.ShopLicense.CopyTo(stream);
                }
                info.ShopLicensePath = "\\ShopUploads\\Documents\\LicenseDoc\\" + uniqueFileName;
                info.ShopLicense = null;
            }
            info.AdminId = id;
            await _context.TblAdminInfos.AddAsync(info);
            await _context.SaveChangesAsync();
            return new { success = true, message = "Documents added successfully" };
            }
        }
}
