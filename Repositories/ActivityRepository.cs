using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
namespace WMS_Application.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly dbMain _context;

        public ActivityRepository(dbMain context)
        {
            _context = context;
        }

        public object AddNewActivity(int id, int roleId, string type, string desc)
        {
            TblActivityLog activity = new TblActivityLog()
            {
                UserId = id,
                Role = roleId,
                ActivityType = type,
                Description = desc
            };
            _context.TblActivityLogs.Add(activity);
            _context.SaveChanges();

            return new { success = true, message = "Activity logged successfully" };
        }
    }
}
