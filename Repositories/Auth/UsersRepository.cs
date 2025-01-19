using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
namespace WMS_Application.Repositories.Auth
{
    public class UsersRepository : UsersInterface
    {
        private readonly dbMain _context;
        private readonly EmailSenderInterface _emailSender;

        public UsersRepository(dbMain context, EmailSenderInterface emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<bool> IsUsernameExists(string Username)
        {
            return await _context.Users.AnyAsync(x => x.Username == Username);
        }

        public async Task<bool> IsEmailExists(string Email)
        {
            return await _context.Users.AnyAsync(x => x.Email == Email);
        }

        //This method will send verification Otp to user's email and save userData into db
        public async Task<object> SaveUsers(User user)
        {
            //Encrypting password using bcrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            //Generating OTP and sending it to user's email
            user.Otp = _emailSender.GenerateOtp();
            user.OtpExpiry = DateTime.Now.AddMinutes(5);
            user.IsVerified = false;
            string subj = "OTP Verification!!";
            await _emailSender.SendEmailAsync(user.Email, subj, user.Otp);

            //Saving all given data to db
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new { success = true, message = "Registered Successfully, OTP sent to email, please verify to move forward" };
        }

        //This method will check if the otp given by user is correct or not
        public async Task<bool> OtpVerification(string otp)
        {
            return await _context.Users.AnyAsync(x => x.Otp == otp && x.OtpExpiry > DateTime.Now);
        }

        //This method will update the status of user to verified
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


        //Saving other data after OTP verification
        public async Task<object> SaveMoreDetails(User user)
        {
            var UpdatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

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

            _context.Users.Update(UpdatedUser);
            await _context.SaveChangesAsync();

            return new { success = true, message = "More details have beed saved successfully" };
        }

        //Retrieving USerID using Email to store it into ShopDetails
        public async Task<int?> GetUserIdByEmail(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => (int?)u.UserId)
                .FirstOrDefaultAsync(); // Retrieves the first match or null
        }


        public async Task<object> SaveShopDetails(Shop shop, int id)
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
            await _context.Shops.AddAsync(shop);
            await _context.SaveChangesAsync();

            return new { success = true, message = "Shop data added successfully enjoyy" };
        }
    }
}
