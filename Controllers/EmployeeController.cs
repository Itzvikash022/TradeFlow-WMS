using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeRepository _employee;
        public EmployeeController(ISidebarRepository sidebar, IEmployeeRepository employee) : base(sidebar)
        {
            _employee = employee;
        }
        [Route("Employees")]
        public async Task<IActionResult> Index()
        {
            return View(await _employee.GetAllEmployees());
        }
        
        public IActionResult SaveEmployee()
        {
            return View();
        }
    }
}
