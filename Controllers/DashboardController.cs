using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class DashboardController : Controller
    {
        private readonly dbMain _context;
        private readonly ISidebarRepository _sidebar;
        public DashboardController(dbMain context, ISidebarRepository sidebar)
        {
            _context = context;
            _sidebar = sidebar;
        }   


        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            int roleId = (int) HttpContext.Session.GetInt32("UserRoleId");
            var tabs = await _sidebar.GetTabsByRoleIdAsync(roleId);
            return View(tabs);
        }
    }
}
