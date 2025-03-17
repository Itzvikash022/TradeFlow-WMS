using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class MasterRepository : IMasterRepository
    {
        private readonly dbMain _context;
        public MasterRepository(dbMain context)
        {
            _context = context;
        }

    }
}
