using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ISidebarRepository _sidebar;
        public BaseController(ISidebarRepository sidebar)
        {
            _sidebar = sidebar;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result; // Sync for simplicity
            ViewBag.SidebarTabs = tabs;
            base.OnActionExecuting(context);
        }
    }
}
