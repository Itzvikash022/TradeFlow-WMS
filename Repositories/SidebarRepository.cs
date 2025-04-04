using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using iText.Layout.Element;


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
                              where p.RoleId == roleId && t.IsActive == true
                              select new SidebarModel
                              {
                                  TabId = t.TabId,
                                  TabName = t.TabName,
                                  ParentId = t.ParentId,
                                  TabUrl = t.TabUrl,
                                  IconPath = t.IconPath,
                                  IsActive = t.IsActive,
                                  PermissionType = p.PermissionType

                              }).ToListAsync();

            // Group the tabs into a hierarchical structure (parent-child)
            var tabHierarchy = tabs
                .Where(tab => tab.ParentId == null && tab.IsActive == true)
                .Select(tab => new SidebarModel
                {
                    TabId = tab.TabId,
                    TabName = tab.TabName,
                    ParentId = tab.ParentId,
                    TabUrl = tab.TabUrl,
                    IconPath = tab.IconPath,
                    PermissionType = tab.PermissionType,
                    //IsActive = tab.IsActive,
                    SubTabs = tabs.Where(sub => sub.ParentId == tab.TabId && sub.IsActive == true).ToList()
                }).ToList();

            return tabHierarchy;
        }

        public TblUser GetUserById(int userId)
        {
            return _context.TblUsers
                .Where(x => x.UserId == userId)
                .Select(user => new TblUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    ProfileImgPath = user.ProfileImgPath,
                    Designation = _context.TblRoles
                        .Where(r => r.RoleId == user.RoleId)
                        .Select(r => r.RoleName)
                        .FirstOrDefault()
                })
                .FirstOrDefault();
        }
        
        public TblUser GetCompanyById(int compId)
        {
            return _context.TblCompanies
                .Where(x => x.CompanyId == compId)
                .Select(comp => new TblUser
                {
                    UserId = comp.CompanyId,
                    Username = comp.CompanyName,
                    ProfileImgPath = comp.CompanyLogo,
                    Designation = _context.TblRoles
                        .Where(r => r.RoleId == 5)
                        .Select(r => r.RoleName)
                        .FirstOrDefault()
                })
                .FirstOrDefault();
        }



    }
}
