using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class PermisionHelperRepository : IPermisionHelperRepository
    {
        private readonly dbMain _context;

        // Initialize with IHttpContextAccessor
        public PermisionHelperRepository(dbMain context)
        {
            _context = context;
        }
        public string HasAccess(string tabName, int roleId)
        {
            var currentTabId = _context.TblTabs.Where(x => x.TabName == tabName).Select(y => y.TabId).FirstOrDefault();
            var currentTabPermissions = GetTabByRole(roleId, currentTabId);
            return currentTabPermissions?.PermissionType;
        }

        public SidebarModel GetTabByRole(int roleId, int currentTabId)
        {
            return _context.TblPermissions.Where(x => x.RoleId == roleId && x.IsActive == true && x.TabId == currentTabId).Select(y => new SidebarModel
            {
                TabId = y.TabId,
                IsActive = y.IsActive,
                PermissionType = y.PermissionType
            }).FirstOrDefault();
        }
    }
}
