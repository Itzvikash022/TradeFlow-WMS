using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly dbMain _db;
        private readonly UsersInterface _users;
        public HomeController(ILogger<HomeController> logger, dbMain db, UsersInterface users)
        {
            _logger = logger;
            _db = db;
            _users = users;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                if(await _users.IsUsernameExists(user.Username))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }
                if(await _users.IsEmailExists(user.Email))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                return Json(new { success = true, message = "User registered successfully" });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString);
                return Json(new { success = false, message = "Unkown error occured" });

            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
