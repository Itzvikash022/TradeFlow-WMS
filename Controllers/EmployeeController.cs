using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeRepository _employee;
        private readonly dbMain _context;
        public EmployeeController(ISidebarRepository sidebar,dbMain context, IEmployeeRepository employee) : base(sidebar)
        {
            _employee = employee;
            _context = context;
        }
        [Route("Employees")]
        public async Task<IActionResult> Index()
        {
            string id = ((int)HttpContext.Session.GetInt32("UserId")).ToString();
            return View(await _employee.GetAllEmployees(id));
        }

        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid admin ID." });
            }

            // Try deleting the admin from the database or perform your logic here
            try
            {
                var emp = _context.TblUsers.Find(id);
                _context.TblUsers.Remove(emp);
                _context.SaveChanges();

                // If successful, redirect to Index
                return Json(new { success = true, message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting admin: " + ex.Message);
                return Json(new { success = false, message = "Error deleting admin." });
            }
        }
    }
}
    