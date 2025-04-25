using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;

namespace WMS_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly dbMain _context;

        public HomeController(dbMain context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int totalUsers = _context.TblUsers.Count(x => x.RoleId == 2) + _context.TblCompanies.Count();
            ViewBag.TotalUsers = totalUsers;

            int totalTransaction = _context.TblOrders.Sum(x => (int?)x.TotalAmount) ?? 0;
            string formattedTransaction = (totalTransaction / 1000000.0m).ToString("0.#") + "M+";
            ViewBag.TotalTransaction = formattedTransaction;

            int orderCount = _context.TblOrders.Count();
            ViewBag.OrderCount = orderCount;

            int productCount = _context.TblProducts.Count();
            ViewBag.ProductCount = productCount;

            return View();
        }
    }
}
