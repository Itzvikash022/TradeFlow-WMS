using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Protocol.Plugins;
using System.IO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WMS_Application.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;
        public CompanyRepository(dbMain context, IEmailSenderRepository emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public List<TblCompany> GetAllCompanies()
        {
            var comp = _context.TblCompanies.Where(x=>x.IsDeleted == false && x.IsActive == true).ToList();
            return comp;
        }
        public async Task<bool> IsEmailExists(string Email)
        {
            return await _context.TblCompanies.AnyAsync(x => x.Email == Email);
        }
        public async Task<object> AuthenticateUser(string email, string password)
        {
            //Fetching user by email or username
            var company = await _context.TblCompanies.FirstOrDefaultAsync(u => u.Email == email);

            if (company == null)
            {
                return new { success = false, message = "Company with this email not found" };
            }
            
            if (company.IsDeleted == true)
            {
                return new { success = false, message = "Company has been deleted" };
            }
            
            if (company.IsActive == false)
            {
                return new { success = false, message = "Company has been Restricted" };
            }

            //Comparing hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, company.PasswordHash);
            if (isPasswordValid)
            {
                return new { success = true, message = "You are successfully logged in" };
            }
            else
            {
                return new { success = false, message = "Invalid Email or Password" };
            }
        }

        public List<TblCompany> GetCompanyReports()
        {
            var companyData = _context.TblCompanies.Where(x => x.IsDeleted == false).OrderByDescending(x=> x.CreatedAt).ToList();
            foreach(var company in companyData)
            {
                company.PasswordHash = null;
                company.OrderCount = _context.TblOrders.Where( x => x.SellerId == company.CompanyId && x.OrderStatus == "Success").Count();
            }
            return companyData;
        }

        public async Task<object> SaveCompany(TblCompany company)
        {
            var UpdatedCompany = await _context.TblCompanies.FirstOrDefaultAsync(u => u.CompanyId == company.CompanyId);
            string imgPath = "";
            if (company.LogoFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(company.LogoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\CompanyUploads");
                Directory.CreateDirectory(uploadsFolder); // Ensures folder exists
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + company.LogoFile.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    company.LogoFile.CopyTo(stream);
                }


                imgPath = "\\CompanyUploads\\" + uniqueFileName;
            }


            if (company.CompanyId > 0)
            {
                UpdatedCompany.CompanyId = company.CompanyId;
                UpdatedCompany.CompanyName = company.CompanyName;
                UpdatedCompany.Email = company.Email;
                UpdatedCompany.PhoneNumber = company.PhoneNumber;
                UpdatedCompany.Gst = company.Gst;
                UpdatedCompany.State = company.State;
                UpdatedCompany.City = company.City;
                UpdatedCompany.Pincode = company.Pincode;
                UpdatedCompany.Address = company.Address;
                if (company.LogoFile!= null)
                {
                    UpdatedCompany.CompanyLogo= imgPath;
                }
            }

            company.LogoFile = null;
            company.CompanyLogo = imgPath;
            string msg = "";
            if (company.CompanyId > 0)
            {
                _context.Update(UpdatedCompany);
                msg = "Company data has been updated successfully";
            }
            else
            {
                company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(company.PasswordHash);
                _context.AddAsync(company);
                msg = "New Company has been added successfully";
            }
            await _context.SaveChangesAsync();

            return new { success = true, message = msg, path = company.Path };
        }

        public async Task<object> AddProduct(TblProduct product)
        {
            product.UnregCompanyId = 0;
            var UpdatedProduct = await _context.TblProducts.FirstOrDefaultAsync(u => u.ProductId == product.ProductId);
            string imgPath = "";
            if (product.ProductImage!= null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                // Maximum allowed file size (5 MB in bytes)
                //const long maxFileSize = 5  1024  1024;

                // Get file extension and check if it is allowed
                string fileExtension = Path.GetExtension(product.ProductImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ProductUploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ProductImage.FileName;

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    product.ProductImage.CopyTo(stream);
                }

                imgPath = "\\ProductUploads\\" + uniqueFileName;
            }
            if (product.ProductId > 0)
            {
                UpdatedProduct.UnregCompanyId = product.UnregCompanyId;
                UpdatedProduct.ProductName = product.ProductName;
                UpdatedProduct.Category = product.Category;
                UpdatedProduct.ProductQty = product.ProductQty;
                UpdatedProduct.PricePerUnit = product.PricePerUnit;
                UpdatedProduct.CompanyId = product.CompanyId;
                UpdatedProduct.Manufacturer = product.Manufacturer;
                UpdatedProduct.LastUpdateDate = DateTime.Now;
                if (product.ProductImage != null)
                {
                    UpdatedProduct.ProductImagePath = imgPath;
                }
            }

            product.ProductImage = null;
            product.ProductImagePath = imgPath;
            product.LastUpdateDate = DateTime.Now;

            string msg = "";
            if (product.ProductId > 0)
            {
                _context.TblProducts.Update(UpdatedProduct);
                msg = "Product data has been updated successfully";
            }
            else
            {
                _context.TblProducts.AddAsync(product);
                msg = "New Product has been added successfully";
            }
            await _context.SaveChangesAsync();

            return new { success = true, message = msg };
        }
    }
}
