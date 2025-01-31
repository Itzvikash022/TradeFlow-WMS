using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductRepository _product;
        public ProductController(ISidebarRepository sidebar, IProductRepository product) : base(sidebar)
        {
            _product = product;
        }
        [Route("Products")]
        public async Task<IActionResult> Index()
        {
            return View(await _product.GetAllProducts());
        }

        public IActionResult SaveProduct()
        {
            return View();
        }
    }
}
