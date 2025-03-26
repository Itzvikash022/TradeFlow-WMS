using Microsoft.AspNetCore.Mvc;

namespace WMS_Application.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult UnauthorisedAccess()
        {
            return View();
        }
    }
}
