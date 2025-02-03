using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<TblCompany>> GetAllCompanies();
    }
}
