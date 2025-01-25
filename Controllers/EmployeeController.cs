using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
   
    public class EmployeeController : BaseController
    {
        public EmployeeController(ISidebarRepository sidebar) : base(sidebar)
        {

        }
        [Route("Employees")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
