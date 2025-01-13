using Microsoft.AspNetCore.Mvc;

namespace WMS_Application.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        
        public IActionResult OtpCheck()
        {
            return View();
        }


    }
}
