using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;

namespace WMS_Application.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly dbMain _context;
        public CustomerRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<object> SaveCustomer(TblCustomer customer)
        {
            await _context.TblCustomers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return new { message = "New Customer added successfully" };
        }
    }

}
