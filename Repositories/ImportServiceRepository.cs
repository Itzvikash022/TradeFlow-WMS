using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class ImportServiceRepository : IImportServiceRepository
    {
        private readonly dbMain _context;
        private readonly IProductRepository _product;
        private readonly IAdminsRepository _admin;
        private readonly IUsersRepository _user;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ImportServiceRepository(dbMain context, IProductRepository product, IAdminsRepository admin, IUsersRepository user, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _product = product;
            _admin = admin;
            _user = user;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public byte[] GenerateSampleStockExcel(bool company)
        {
            // Create a new workbook & sheet
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sample");

            // Add headers

            if (company)
            {
                sheet.Cell(1, 1).Value = "Product Name";
                sheet.Cell(1, 2).Value = "Category";  // Dropdown applied here
                sheet.Cell(1, 3).Value = "Price per Unit";
                sheet.Cell(1, 4).Value = "Manufacturer";


                sheet.Cell(1, 5).Value = "Quantity";
                sheet.Cell(1, 6).Value = "ProfileImagePath";

            }
            else
            {
                sheet.Cell(1, 1).Value = "Product Name";
                sheet.Cell(1, 2).Value = "Category";  // Dropdown applied here
                sheet.Cell(1, 3).Value = "Price per Unit";
                sheet.Cell(1, 4).Value = "Manufacturer";
                sheet.Cell(1, 5).Value = "Quantity";
                sheet.Cell(1, 6).Value = "Shop Price";
                sheet.Cell(1, 7).Value = "Company Name";
                sheet.Cell(1, 8).Value = "ProfileImagePath";
            }

            // Fetch categories from DB
            var categories = _context.TblProductCategories
                                     .Select(c => c.ProductCategory)
                                     .ToList();

            // Add categories to a hidden sheet
            var categorySheet = workbook.Worksheets.Add("Categories");

            for (int i = 0; i < categories.Count; i++)
            {
                categorySheet.Cell(i + 1, 1).Value = categories[i];
            }

            // Hide the category sheet (so users don't see it)
            categorySheet.Hide();

            // Apply dropdown list to Category column (B2:B100)
            var categoryRange = sheet.Range("B2:B100");
            var categoryValidation = categoryRange.CreateDataValidation();
            categoryValidation.List(categorySheet.Range($"A1:A{categories.Count}"));

            // Save to MemoryStream & return as file
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

        }


        //COMPANY PRODUCT IMPORT
        public async Task<byte[]> CompanyProcessStockImport(IFormFile file, int companyId)
        {
            var resultList = new List<ImportStatusModel>();
            int count = 0;
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheet(1);
                    var rows = sheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        var productName = row.Cell(1).GetString();
                        var categoryName = row.Cell(2).GetString();
                        var manufacturer = row.Cell(4).GetString();
                        var productImagePath = row.Cell(6).GetString();

                        int pricePerUnit;
                        if (!int.TryParse(row.Cell(3).GetString(), out pricePerUnit))
                        {
                            pricePerUnit = 0; // Or handle error (e.g., log it, skip row)
                        }
                        //var pricePerUnit = row.Cell(3).GetValue<int>();
                        //var quantity = row.Cell(5).GetValue<int>();
                        //var shopPrice = row.Cell(6).GetValue<int>();

                        int quantity;
                        if (!int.TryParse(row.Cell(5).GetString(), out quantity))
                        {
                            quantity = 0;
                        }



                        var importStatus = new ImportStatusModel
                        {
                            Name = productName,
                            Status = "Pending",
                            Message = ""
                        };

                        try
                        {
                            // 🛠 Validate Data
                            var category = _context.TblProductCategories.FirstOrDefault(c => c.ProductCategory == categoryName);
                            if (category == null)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Invalid Category";
                            }
                            else if (pricePerUnit <= 0)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Price must be greater than 0";
                            }
                            else if (quantity <= 0)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Quantity must be greater than 0";
                            }
                            else if (string.IsNullOrEmpty(productName))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Product name must have a value";
                            }
                            else if (string.IsNullOrEmpty(manufacturer))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Manufacturer must have a value";
                            }
                            else
                            {


                                // ✅ Insert into tblProducts

                                var product = new TblProduct
                                {
                                    ProductName = productName,
                                    Category = category.ProdCatId,
                                    PricePerUnit = pricePerUnit,
                                    Manufacturer = manufacturer,
                                    LastUpdateDate = DateTime.Now,
                                    CompanyId = companyId,
                                    ProductQty = quantity,
                                    UnregCompanyId = 0,
                                    ProductImagePath = @"\CompanyUploads\" + productImagePath
                                };

                                _context.TblProducts.Add(product);
                                _context.SaveChanges();
                                count++;
                                importStatus.Status = "Success";
                            }
                        }
                        catch (Exception ex)
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = ex.Message;
                        }

                        resultList.Add(importStatus);
                    }
                }
            }
            _httpContextAccessor.HttpContext.Session.SetInt32("ImportedProductCount", count);
            return GenerateStatusReport(resultList);
        }

        //SHOP PRODUCT IMPORT

        public async Task<byte[]> ShopProcessStockImport(IFormFile file, int userId, int shopId)
        {
            int count = 0;
            var resultList = new List<ImportStatusModel>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheet(1);
                    var rows = sheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        var productName = row.Cell(1).GetString();
                        var categoryName = row.Cell(2).GetString();
                        var manufacturer = row.Cell(4).GetString();
                        var companyName = row.Cell(7).GetString();
                        var productImagePath = row.Cell(8).GetString();

                        int pricePerUnit;
                        if (!int.TryParse(row.Cell(3).GetString(), out pricePerUnit))
                        {
                            pricePerUnit = 0; // Or handle error (e.g., log it, skip row)
                        }
                        //var pricePerUnit = row.Cell(3).GetValue<int>();
                        //var quantity = row.Cell(5).GetValue<int>();
                        //var shopPrice = row.Cell(6).GetValue<int>();

                        int quantity;
                        if (!int.TryParse(row.Cell(5).GetString(), out quantity))
                        {
                            quantity = 0;
                        }

                        // Safely parse Shop Price
                        int shopPrice;
                        if (!int.TryParse(row.Cell(6).GetString(), out shopPrice))
                        {
                            shopPrice = 0;
                        }


                        var importStatus = new ImportStatusModel
                        {
                            Name = productName,
                            Status = "Pending",
                            Message = ""
                        };

                        try
                        {
                            // 🛠 Validate Data
                            var category = _context.TblProductCategories.FirstOrDefault(c => c.ProductCategory == categoryName);
                            if (category == null)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Invalid Category";
                            }
                            else if (pricePerUnit <= 0 || shopPrice <= 0)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Price must be greater than 0";
                            }
                            else if (quantity <= 0)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Quantity must be greater than 0";
                            }
                            else if (string.IsNullOrEmpty(productName))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Product name must have a value";
                            }
                            else if (string.IsNullOrEmpty(companyName))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Company name must have a value";
                            }
                            else if (string.IsNullOrEmpty(manufacturer))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Manufacturer must have a value";
                            }
                            else
                            {

                                // ✅ Insert into tblUnregCompany

                                var UnregCompanyId = await _product.SaveUnregCompanyAsync(companyName, userId);


                                // ✅ Insert into tblProducts

                                var product = new TblProduct
                                {
                                    ProductName = productName,
                                    Category = category.ProdCatId,
                                    PricePerUnit = pricePerUnit,
                                    Manufacturer = manufacturer,
                                    LastUpdateDate = DateTime.Now,
                                    UnregCompanyId = UnregCompanyId,
                                    CompanyId = 0,
                                    ProductImagePath = @"\ProductUploads\" + productImagePath
                                };

                                _context.TblProducts.Add(product);
                                _context.SaveChanges();
                                count++;
                                // ✅ Insert into tblStock
                                await _product.SaveStockAsync(product.ProductId, shopId, quantity, shopPrice);

                                importStatus.Status = "Success";
                            }
                        }
                        catch (Exception ex)
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = ex.Message;
                        }

                        resultList.Add(importStatus);
                    }
                }
            }
            _httpContextAccessor.HttpContext.Session.SetInt32("ImportedProductCount", count);

            return GenerateStatusReport(resultList);
        }


        //SHOP EMPLOYEE IMPORT

        public byte[] GenerateEmployeeSampleExcel()
        {
            // Create a new workbook & sheet
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sample");

            // Add headers

            sheet.Cell(1, 1).Value = "First Name";
            sheet.Cell(1, 2).Value = "Last Name";  // Dropdown applied here
            sheet.Cell(1, 3).Value = "Username";
            sheet.Cell(1, 4).Value = "Password";
            sheet.Cell(1, 5).Value = "Phone Number";
            sheet.Cell(1, 6).Value = "Email";
            sheet.Cell(1, 7).Value = "Designation";
            sheet.Cell(1, 8).Value = "ProfileImagePath";

            // Fetch categories from DB
            var designation = _context.TblRoles
                                     .Where(x => x.RoleId > 2 && x.RoleId != 5)
                                     .Select(c => c.RoleName)
                                     .ToList();

            // Add categories to a hidden sheet
            var desigSheet = workbook.Worksheets.Add("Designation");

            for (int i = 0; i < designation.Count; i++)
            {
                desigSheet.Cell(i + 1, 1).Value = designation[i];
            }

            // Hide the category sheet (so users don't see it)
            desigSheet.Hide();

            // Apply dropdown list to Category column (B2:B100)
            var desigRange = sheet.Range("G2:G100");
            var desigValidation = desigRange.CreateDataValidation();
            desigValidation.List(desigSheet.Range($"A1:A{designation.Count}"));

            // Save to MemoryStream & return as file
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

        }



        public async Task<byte[]> EmployeeProcessImport(IFormFile file, int userId)
        {
            int count = 0;
            var resultList = new List<ImportStatusModel>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheet(1);
                    var rows = sheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        var firstName = row.Cell(1).GetString();
                        var lastName = row.Cell(2).GetString();
                        var userName = row.Cell(3).GetString();
                        var password = row.Cell(4).GetString();
                        var phoneNumber = row.Cell(5).GetString();
                        var email = row.Cell(6).GetString();
                        var designation = row.Cell(7).GetString();
                        var profileImagePath = row.Cell(8).GetString();

                        var importStatus = new ImportStatusModel
                        {
                            Name = userName,
                            Status = "Pending",
                            Message = ""
                        };

                        try
                        {
                            // 🛠 Validate Data
                            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "First and Last Name are required.";
                            }
                            else if (string.IsNullOrEmpty(userName))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Username is required.";
                            }
                            else if (string.IsNullOrEmpty(password))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Password is required.";
                            }
                            else if (password.Length < 8)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Password must be at least 8 characters.";
                            }
                            else if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 10)
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Invalid phone number.";
                            }
                            else if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Invalid email format.";
                            }
                            else if (string.IsNullOrEmpty(designation))
                            {
                                importStatus.Status = "Failed";
                                importStatus.Message = "Designation is required.";
                            }
                            else
                            {
                                // 🛠 Fetch required data BEFORE making async calls
                                var role = await _context.TblRoles
                                    .Where(x => x.RoleName == designation)
                                    .Select(x => x.RoleId)
                                    .FirstOrDefaultAsync();

                                if (role == 0)
                                {
                                    importStatus.Status = "Failed";
                                    importStatus.Message = "Invalid Role";
                                }
                                else if (await _user.IsUsernameExists(userName))
                                {
                                    importStatus.Status = "Failed";
                                    importStatus.Message = "Username already exists";
                                }
                                else if (await _user.IsEmailExists(email))
                                {
                                    importStatus.Status = "Failed";
                                    importStatus.Message = "Email already exists";
                                }
                                else
                                {
                                    // ✅ Insert into tblUsers
                                    var user = new TblUser
                                    {
                                        FirstName = firstName,
                                        LastName = lastName,
                                        Username = userName,
                                        PasswordHash = password,
                                        Email = email,
                                        PhoneNumber = phoneNumber,
                                        RoleId = role,
                                        CreatedAt = DateTime.Now,
                                        AdminRef = userId,
                                        IsVerified = false,
                                        IsActive = true,
                                        IsDeleted = false,
                                        VerifiedBy = userId.ToString(),
                                        ProfileImgPath = @"\UserUploads\" + profileImagePath
                                    };

                                    await _admin.SaveUsers(user); // Ensure SaveUsers is properly async

                                    importStatus.Status = "Success";
                                    count++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            importStatus.Status = "Failed";
                            importStatus.Message = $"Error: {ex.Message}";
                        }


                        resultList.Add(importStatus);
                    }
                }
            }

            _httpContextAccessor.HttpContext.Session.SetInt32("ImportedEmployeeCount", count);
            return GenerateStatusReport(resultList);
        }





        //Report generation
        public byte[] GenerateStatusReport(List<ImportStatusModel> results)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Import Status");

                // Headers
                sheet.Cell("A1").Value = "Name";
                sheet.Cell("B1").Value = "Status";
                sheet.Cell("C1").Value = "Message";

                int row = 2;
                foreach (var result in results)
                {
                    sheet.Cell(row, 1).Value = result.Name;
                    sheet.Cell(row, 2).Value = result.Status;
                    sheet.Cell(row, 3).Value = result.Message;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }







    }
}
