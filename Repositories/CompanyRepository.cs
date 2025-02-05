using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WMS_Application.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly dbMain _context;
        public CompanyRepository(dbMain context)
        {
            _context = context;
        }
        public async Task<List<TblCompany>> GetAllCompanies()
        {
            var comp = _context.TblCompanies.ToList();
            return comp;
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

                UpdatedCompany.CompanyName = company.CompanyName;
                UpdatedCompany.ContactPerson = company.ContactPerson;
                UpdatedCompany.Email = company.Email;
                UpdatedCompany.Gst = company.Gst;
                UpdatedCompany.State = company.State;
                UpdatedCompany.City = company.City;
                UpdatedCompany.Pincode = company.Pincode;
                UpdatedCompany.Location = company.Location;
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
                _context.AddAsync(company);
                msg = "New Company has been added successfully";
            }
            await _context.SaveChangesAsync();

            return new { success = true, message = msg };
        }

        public async Task<object> AddProduct(TblProduct product)
        {
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

                UpdatedProduct.ProductName = product.ProductName;
                UpdatedProduct.Category = product.Category;
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
