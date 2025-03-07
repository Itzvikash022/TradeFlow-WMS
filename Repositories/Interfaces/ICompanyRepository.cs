using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        List<TblCompany> GetAllCompanies();
        Task<object> SaveCompany(TblCompany company);
        Task<object> AddProduct(TblProduct product);
        Task<bool> IsEmailExists(string Email);
        Task<object> AuthenticateUser(string email, string password);
        List<TblCompany> GetCompanyReports();
    }
}
