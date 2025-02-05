using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly dbMain _context;
        public EmployeeRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<List<TblUser>> GetAllEmployees(string id)
        {
            var employees = from employee in _context.TblUsers
                            join role in _context.TblRoles on employee.RoleId equals role.RoleId
                            where employee.RoleId != 1 && employee.RoleId != 2 && employee.AdminRef == id
                            select new TblUser
                            {
                                UserId = employee.UserId,
                                Username = employee.Username,
                                FirstName = employee.FirstName,
                                LastName = employee.LastName,
                                Email = employee.Email,
                                RoleId = employee.RoleId,
                                PhoneNumber = employee.PhoneNumber,
                                CreatedAt = employee.CreatedAt,
                                ProfileImgPath = employee.ProfileImgPath,
                                Designation = role.RoleName
                            };
            return employees.ToList();
        }
    }
}
