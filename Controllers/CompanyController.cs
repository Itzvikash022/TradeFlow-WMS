using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

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
            return View(await _company.GetAllCompanies());
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
            return Ok(await _company.SaveCompany(model));
        }

        [HttpGet]
        public async Task<IActionResult> AddProducts(int? id)
        {
            ViewBag.company = await _context.TblCompanies.ToListAsync();
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
            return Ok(await _company.AddProduct(product));  
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDetails(int id)
        {
            TblCompany company = await _context.TblCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();
            return View(company);
        }

        [HttpGet]
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
                Console.WriteLine("Error deleting admin: " + ex.Message);
                return Json(new { success = false, message = "Error deleting admin." });
            }
        }
    }
}
