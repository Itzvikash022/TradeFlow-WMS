using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
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
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            // Prevent caching
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Check if user is authenticated
            var httpContext = context.HttpContext;

            // Fetch session values
            int? userId = httpContext.Session.GetInt32("UserId");
            int? companyId = httpContext.Session.GetInt32("CompanyId");
            int? roleId = httpContext.Session.GetInt32("UserRoleId");

            // ✅ Ensure at least one valid login session exists
            if ((userId > 0 && companyId > 0) || roleId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // ✅ Only fetch sidebar tabs if roleId is valid
            if (roleId > 0)
            {
                var tabs = await _sidebar.GetTabsByRoleIdAsync(roleId.Value); // Async call
                ViewBag.SidebarTabs = tabs;

                if(userId > 0)
                {
                    var user = _sidebar.GetUserById((int) userId);
                    ViewBag.UserDetails = user;
                }
                else
                {
                    var company = _sidebar.GetCompanyById((int) companyId);
                    ViewBag.UserDetails = company;
                }
            }

            await next(); // Continue with the action execution
        }
    }
}
