using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly dbMain _context;
        private readonly IPermisionHelperRepository _permission;

        public DashboardController(dbMain context, ISidebarRepository sidebar, IPermisionHelperRepository permision) : base(sidebar)
        {
            _context = context;
            _permission = permision;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return _permission.HasAccess(action, roleId);
        }

        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess("Dashboard", roleId);
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [Route("MyProfile")]
        public async Task<IActionResult> MyProfile()
        {
            var permissionType = GetUserPermission("My Profile");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int UserId = (int)HttpContext.Session.GetInt32("UserId");
                TblUser userData = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == UserId);
                return View(userData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
    }
}
