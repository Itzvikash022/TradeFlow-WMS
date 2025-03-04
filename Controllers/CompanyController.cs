using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WMS_Application.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly dbMain _context;
        private readonly ICompanyRepository _company;
        public CompanyController(ISidebarRepository sidebar, dbMain context, ICompanyRepository company) : base(sidebar)
        {
            _context = context;
            _company = company;
        }
        [Route("Company")]
        public async Task<IActionResult> Index()
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            return View(_company.GetAllCompanies());
        }

        [Route("/MyCompany")]
        public IActionResult MyCompany()
        {
            int companyId = 0;
            TblCompany companyData = new TblCompany();
            if (HttpContext.Session.GetInt32("UserRoleId") == 5)
            {
                companyId = (int) HttpContext.Session.GetInt32("CompanyId");
                companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == companyId); 
                return View(companyData);
            }
            else
            {
                return View(null);
            }
        }

        public IActionResult UpdateCompany(int? id)
        {
            TblCompany companyData = new TblCompany();
            if (HttpContext.Session.GetInt32("UserRoleId") == 5)
            {
                int companyId = (int) HttpContext.Session.GetInt32("CompanyId");
                companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == companyId);
            }
            else if(id > 0)
            {
                companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyId == id);
            }
            
            return View(companyData);
        }


        public async Task<IActionResult> Products()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SaveCompany(int? id)
        {
            TblCompany model = new TblCompany();
            if (id > 0)
            {
                model = await _context.TblCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();    
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCompany(TblCompany model)
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            //model.AddedBy = id;
            return Ok(await _company.SaveCompany(model));
        }

        [HttpGet]
        public async Task<IActionResult> AddProducts(int? id)
        {
            TblProduct model = new TblProduct();
            if(id > 0)
            {
                model = await _context.TblProducts.Where(x => x.ProductId == id).FirstOrDefaultAsync();
            }
            return View(model);
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
            TblCompany company = await _context.TblCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();
            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid admin ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var company = _context.TblCompanies.Find(id);
                _context.TblCompanies.Remove(company);
                _context.SaveChanges();

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
    }
}
