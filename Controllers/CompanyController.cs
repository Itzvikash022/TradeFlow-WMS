using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Composition;
using System.Data;
using System.IO.Compression;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WMS_Application.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly dbMain _context;
        private readonly ICompanyRepository _company;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IPermisionHelperRepository _permission;
        private readonly IExportServiceRepository _export;
        private readonly IImportServiceRepository _import;
        private readonly HttpClient _httpClient;
        private readonly IActivityRepository _activity;

        public CompanyController(ISidebarRepository sidebar, dbMain context, ICompanyRepository company, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export, IImportServiceRepository import, IHttpClientFactory httpClientFactory, IActivityRepository activity) : base(sidebar)
        {
            _context = context;
            _company = company;
            _emailSender = emailSender;
            _permission = permission;
            _export = export;
            _import = import;
            _httpClient = httpClientFactory.CreateClient();
            _activity = activity;
        }


        [HttpGet("/Company/states")]
        public async Task<IActionResult> GetStates()
        {
            var response = await _httpClient.GetStringAsync("http://api.geonames.org/childrenJSON?geonameId=1269750&username=itzvikash");
            return Content(response, "application/json");
        }

        [HttpGet("/Company/cities/{geonameId}")]
        public async Task<IActionResult> GetCities(int geonameId)
        {
            var url = $"http://api.geonames.org/childrenJSON?geonameId={geonameId}&username=itzvikash";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }


        [Route("Company")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Company");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                return View(_company.GetAllCompanies());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public IActionResult ExportCompanyList()
        {
            List<TblCompany> companyList = _company.GetAllCompanies();

            var dataTable = new DataTable("Companies");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Company Name"),
                new DataColumn("State"),
                new DataColumn("City"),
                new DataColumn("Email"),
                new DataColumn("PhoneNumber"),
                new DataColumn("Started On"),
                new DataColumn("Pincode"),
                new DataColumn("Address"),
                new DataColumn("GST")
            });

            foreach (var company in companyList)
            {
                dataTable.Rows.Add(company.CompanyName, company.State, company.City, company.Email, company.PhoneNumber, company.CreatedAt, company.Pincode, company.Address, company.Gst);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "CompanyList");

            if (fileBytes != null)
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                string username = _context.TblUsers.Where(x => x.UserId == id).Select(s => s.Username).FirstOrDefault();
                string type = "Company List Exported";
                string desc = $"{username} exported Company List";
                _activity.AddNewActivity(id, (int)HttpContext.Session.GetInt32("UserRoleId"), type, desc);

            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CompanyList.xlsx");
        }



        [Route("/MyCompany")]
        public IActionResult MyCompany()
        {
            string permissionType = GetUserPermission("My Company");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int companyId = 0;
                TblCompany companyData = new TblCompany();
                if (HttpContext.Session.GetInt32("UserRoleId") == 5)
                {
                    companyId = (int)HttpContext.Session.GetInt32("CompanyId");
                    companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == companyId);
                    return View(companyData);
                }
                else
                {
                    return View(null);
                }
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }


        }


        public IActionResult UpdateCompany(int? id)
        {

            TblCompany companyData = new TblCompany();
            if (HttpContext.Session.GetInt32("UserRoleId") == 5)
            {
                string permissionType = GetUserPermission("My Company");
                if (permissionType == "canEdit" || permissionType == "fullAccess")
                {
                    int companyId = (int)HttpContext.Session.GetInt32("CompanyId");
                    companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == companyId);
                }
                else
                {
                    return RedirectToAction("UnauthorisedAccess", "Error");
                }
            }
            else if (id > 0)
            {
                string permissionType = GetUserPermission("Company");
                if (permissionType == "canEdit" || permissionType == "fullAccess")
                {
                    companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == id);
                }
                else
                {
                    return RedirectToAction("UnauthorisedAccess", "Error");
                }
            }

            return View(companyData);

        }




        [HttpPost]
        public async Task<IActionResult> AddProducts(TblProduct product)
        {
            int CompanyId = (int)HttpContext.Session.GetInt32("CompanyId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string companyName = _context.TblCompanies.Where(x => x.CompanyId == CompanyId).Select(y => y.CompanyName).FirstOrDefault();
            string type = "", desc = "";
            product.CompanyId = CompanyId;
            int prodId = product.ProductId;

            var result = await _company.AddProduct(product);

            TempData["shopProd-toast"] = "Product Stock Updated Successfully";
            TempData["shopProd-toastType"] = "success";

            if (((dynamic)result).success)
            {
                if (prodId > 0)
                {
                    type = "Company Product Update";
                    desc = $"{companyName} updated product details named {product.ProductName}";
                }
                else
                {
                    type = "Company Product Add";
                    desc = $"{companyName} Added a new product details named {product.ProductName}";
                }

                _activity.AddNewActivity(CompanyId, roleId, type, desc);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDetails(int id)
        {
            string permissionType = GetUserPermission("Company");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblCompany company = await _context.TblCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();
                return View(company);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }

        }

        public IActionResult DownloadSampleFile()
        {
            var fileBytes = _import.GenerateSampleStockExcel(true);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleFile.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSampleFile(IFormFile excelFile, IFormFile imageZip)
        {
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (excelFile == null || imageZip == null)
                return BadRequest("Please upload both Excel file and Image ZIP folder.");

            if (!imageZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .zip files are allowed for image upload." });
            }
            // Extract ZIP file
            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\CompanyUploads");
            Directory.CreateDirectory(imageFolderPath);

            using (var zipStream = new MemoryStream())
            {
                await imageZip.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string extension = Path.GetExtension(entry.Name).ToLower();

                        // ❌ Reject non-image files
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, message = "only image files are allowed inside the zip" });
                        }

                        // ✅ Move valid images
                        var filePath = Path.Combine(imageFolderPath, entry.Name);
                        if (!System.IO.File.Exists(filePath))
                        {
                            entry.ExtractToFile(filePath);
                        }
                    }
                }
            }



            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "File is empty." });
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .xlsx files are allowed." });
            }




            try
            {
                int companyId = (int)HttpContext.Session.GetInt32("CompanyId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                var fileBytes = await _import.CompanyProcessStockImport(excelFile, companyId);

                // Convert to Base64 to store temporarily
                string fileBase64 = Convert.ToBase64String(fileBytes);

                if(fileBase64 != null)
                {
                    string type = "Mass Import Company Product";
                    int count = (int)HttpContext.Session.GetInt32("ImportedProductCount");
                    string compName = _context.TblCompanies.Where(x => x.CompanyId == companyId).Select(s => s.CompanyName).FirstOrDefault();
                    string desc = $"{compName} Mass imported {count} new products";

                    _activity.AddNewActivity(companyId, roleId, type, desc);
                }
                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully!",
                    fileData = fileBase64,
                    fileName = "OutputOfSampleFile.xlsx"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid company ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var company = _context.TblCompanies.Find(id);
                string companyName = company.CompanyName;
                company.IsDeleted = true;
                company.IsActive = false;
                _context.TblCompanies.Update(company);
                _context.SaveChanges();

                //Sending email after deletion
                string subject = "Account Deleted!! heehehehehehe";
                string body = "I'm sorry to inform you but your account has been terminated, please contact the support team if you have query regarding it";
                _emailSender.SendEmailAsync(company.Email, subject, body);

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                string username = _context.TblUsers.Where(x => x.UserId == userId).Select(s => s.Username).FirstOrDefault();
                string type = "Company Deleted";
                string desc = $"{username} Deleted Company named : {companyName}";

                _activity.AddNewActivity(userId, (int)HttpContext.Session.GetInt32("UserRoleId"), type, desc);

                // If successful, redirect to Index
                return Json(new { success = true, message = "Company deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting company: " + ex.Message);
                return Json(new { success = false, message = "Error deleting company." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestrictionStatus(int companyId)
        {

            if (companyId == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid company ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                string msg = "", type = "";
                var company = _context.TblCompanies.Find(companyId);
                if ((bool)company.IsActive)
                {
                    company.IsActive = false;
                    msg = "Company Restricted successfully.";
                    type = "Restricted";
                }
                else
                {
                    company.IsActive = true;
                    msg = "Company UnRestricted successfully.";
                    type = "Unrestricted";
                }
                _context.TblCompanies.Update(company);
                _context.SaveChanges();

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                string username = _context.TblUsers.Where(x => x.UserId == userId).Select(s => s.Username).FirstOrDefault();
                type = "Company " + type;
                string desc = $"{username} {type} Company named : {company.CompanyName}";

                _activity.AddNewActivity(userId, (int)HttpContext.Session.GetInt32("UserRoleId"), type, desc);


                // If successful, redirect to Index
                return Json(new { success = true, message = msg });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error Restricting company: " + ex.Message);
                return Json(new { success = false, message = "Error Restricting company." });
            }
        }
    }
}
