using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class OrdersController : BaseController
    {
        private IOrdersRepository _orders;
        public OrdersController(ISidebarRepository sidebar, IOrdersRepository orders) : base(sidebar)
        {
            _orders = orders;
        }
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            return View(await _orders.GetAllOrders());
        }
    }
}
