using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ShopsController : BaseController
    {
        private readonly IShopRepository _shop;
        public ShopsController(ISidebarRepository sidebar, IShopRepository shop) : base(sidebar)
        {
            _shop = shop;
        }

        [Route("Shops")]
        public async Task<IActionResult> Index()
        {
            return View(await _shop.GetAllShops());
        }
    }
}
