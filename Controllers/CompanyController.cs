using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public CompanyController(ISidebarRepository sidebar, dbMain context, ICompanyRepository company, IEmailSenderRepository emailSender, IPermisionHelperRepository permission) : base(sidebar)
        {
            _context = context;
            _company = company;
            _emailSender = emailSender;
            _permission = permission;
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
            string permissionType = GetUserPermission("Company");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblCompany companyData = new TblCompany();
                if (HttpContext.Session.GetInt32("UserRoleId") == 5)
                {
                    int companyId = (int)HttpContext.Session.GetInt32("CompanyId");
                    companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == companyId);
                }
                else if (id > 0)
                {
                    companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == id);
                }

                return View(companyData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }




        [HttpPost]
        public async Task<IActionResult> AddProducts(TblProduct product)
        {
            int CompanyId = (int)HttpContext.Session.GetInt32("CompanyId");
            product.CompanyId = CompanyId;
            return Ok(await _company.AddProduct(product));  
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
                company.IsDeleted = true;
                company.IsActive = false;
                _context.TblCompanies.Update(company);
                _context.SaveChanges();

                //Sending email after deletion
                string subject = "Account Deleted!! heehehehehehe";
                string body = "I'm sorry to inform you but your account has been terminated, please contact the support team if you have query regarding it";
                _emailSender.SendEmailAsync(company.Email, subject, body);

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
                string msg = "";
                var company = _context.TblCompanies.Find(companyId);
                if ((bool)company.IsActive)
                {
                    company.IsActive = false;
                    msg = "Company Restricted successfully.";
                }
                else
                {
                    company.IsActive = true;
                    msg = "Company UnRestricted successfully.";
                }
                _context.TblCompanies.Update(company);
                _context.SaveChanges();

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
