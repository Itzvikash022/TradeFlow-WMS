using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        List<TblCompany> GetAllCompanies(int id);
        Task<object> SaveCompany(TblCompany company);
        Task<object> AddProduct(TblProduct product);
    }
}
