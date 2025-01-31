using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class RolesController : BaseController
    {
        private readonly IRolesRepository _roles;
        public RolesController(ISidebarRepository sidebar, IRolesRepository roles) : base(sidebar)
        {
            _roles = roles;
        }
        [Route("Roles")]
        public async Task<IActionResult> Index()
        {
            return View(await _roles.GetAllRoles());
        }
    }
}
