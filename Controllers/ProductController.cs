using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductRepository _product;
        private readonly dbMain _context;
        private readonly IUsersRepository _users;
        public ProductController(ISidebarRepository sidebar, IProductRepository product, dbMain context, IUsersRepository users) : base(sidebar)
        {
            _product = product;
            _context = context;
            _users = users;
        }
        [HttpGet]
        [Route("Products")]
        public async Task<IActionResult> Index(int? companyId)
        {
            int Id = 0, shopId = 0;
            TblShop ShopData = new TblShop();
            if(HttpContext.Session.GetInt32("CompanyId") != null && HttpContext.Session.GetInt32("UserRoleId") == 5)
            {
                Id = (int) HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                Id = companyId ?? 0;
                int adminId = (int)HttpContext.Session.GetInt32("UserId");
                ShopData = await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == adminId);
                shopId = ShopData.ShopId;
            }
            
            return View(await _product.GetAllProducts(Id, shopId));
        }

        public IActionResult SaveShopProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveShopProduct([FromForm] TblProduct product)
        {
            int qty = product.ProductQty;
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int shopId = (int)HttpContext.Session.GetInt32("ShopId");
            int UnregCompanyId =await _product.SaveUnregCompanyAsync(product.CompanyName, userId);
            product.UnregCompanyId = UnregCompanyId;
            var result =await _product.SaveShopProductAsync(product,userId,shopId);
            
            if (((dynamic)result).success)
            {
                int newProductId = ((dynamic)result).productId;

                await _product.SaveStockAsync(newProductId,shopId, qty, product.ShopPrice);
            }
            return Json(result);
        }
    }
}
