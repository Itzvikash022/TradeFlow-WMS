using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace WMS_Application.Repositories
{
    public class SidebarRepository : ISidebarRepository
    {
        private readonly dbMain _context;

        public SidebarRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId)
        {
            var tabs = await (from t in _context.TblTabs
                              join p in _context.TblPermissions on t.TabId equals p.TabId
                              where p.RoleId == roleId
                              select new SidebarModel
                              {
                                  TabId = t.TabId,
                                  TabName = t.TabName,
                                  ParentId = t.ParentId,
                                  TabUrl = t.TabUrl,
                                  IconPath = t.IconPath
                                  //IsActive = t.IsActive
                              }).ToListAsync();

            // Group the tabs into a hierarchical structure (parent-child)
            var tabHierarchy = tabs
                .Where(tab => tab.ParentId == null)
                .Select(tab => new SidebarModel
                {
                    TabId = tab.TabId,
                    TabName = tab.TabName,
                    ParentId = tab.ParentId,
                    TabUrl = tab.TabUrl,
                    IconPath = tab.IconPath,
                    //IsActive = tab.IsActive,
                    SubTabs = tabs.Where(sub => sub.ParentId == tab.TabId).ToList()
                }).ToList();

            return tabHierarchy;
        }


    }
}
