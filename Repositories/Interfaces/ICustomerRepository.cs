using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<object> SaveCustomer(TblCustomer customer);
    }
}
