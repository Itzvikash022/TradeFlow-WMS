using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

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
    }
}
