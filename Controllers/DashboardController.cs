using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly dbMain _context;
        //private readonly ISidebarRepository _sidebar;
        public DashboardController(dbMain context, ISidebarRepository sidebar) : base(sidebar)
        {
            _context = context;
            //_sidebar = sidebar;
        }   


        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
