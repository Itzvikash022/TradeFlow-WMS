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

        public async Task<IActionResult> SaveCompany(int? id)
        {
            TblCompany model = new TblCompany();
            if (id > 0)
            {
                model = await _context.TblCompanies.Where(x => x.CompanyId == id).FirstOrDefaultAsync();    
            }
            return View(model);
        }

        public async Task<IActionResult> SaveCompany(TblCompany model)
        {

            return RedirectToAction("Index");
        }
    }
}
