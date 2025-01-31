using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
namespace WMS_Application.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly dbMain _context;
        public RolesRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<List<TblRole>> GetAllRoles()
        {
            return _context.TblRoles.ToList();
        }
    }
}
