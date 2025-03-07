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

        public async Task<List<TblUser>> GetAllEmployees(int id)
        {
            var employees = from employee in _context.TblUsers
                            join role in _context.TblRoles on employee.RoleId equals role.RoleId
                            where employee.RoleId != 1 && employee.RoleId != 2 && employee.AdminRef == id && employee.IsDeleted == false && employee.IsActive == true
                            select new TblUser
                            {
                                UserId = employee.UserId,
                                Username = employee.Username,
                                FirstName = employee.FirstName,
                                DateOfBirth = employee.DateOfBirth,
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

        public List<TblUser> GetEmployeeReports(int userId)
        {
            var employeeData = _context.TblUsers.Where(x => x.IsDeleted == false && x.RoleId > 2 && x.RoleId != 5 && x.AdminRef == userId).ToList();

            foreach(var employee in employeeData)
            {
                employee.Designation = _context.TblRoles.Where(x => x.RoleId == employee.RoleId).Select(y=> y.RoleName).FirstOrDefault();
                var admin = _context.TblUsers.Where(x => x.UserId == employee.AdminRef).FirstOrDefault();
                employee.Head = admin.FirstName + " " + admin.LastName;
                employee.ShopName = _context.TblShops.Where(x => x.AdminId == admin.UserId).Select(y => y.ShopName).FirstOrDefault();
            }
            return employeeData;
        }
    }
}
