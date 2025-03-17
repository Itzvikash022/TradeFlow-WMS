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
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if (HttpContext.Session.GetInt32("CompanyId") != null && roleId == 5)
            {
                Id = (int) HttpContext.Session.GetInt32("CompanyId");
            }
            else
            {
                Id = companyId ?? 0;
                int adminId = (int)HttpContext.Session.GetInt32("UserId");
                if(roleId > 2 && roleId != 5)
                {
                    adminId = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
                }
                ShopData = await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == adminId);
                shopId = ShopData.ShopId;
            }
            
            return View(await _product.GetAllProducts(Id, shopId));
        }

        [HttpGet]
        public IActionResult SaveShopProduct(int? id)
        {
            List<TblProductCategory> prodCat = _context.TblProductCategories.Where(x => x.IsActive == true).ToList();
            ViewBag.ProductCategory = prodCat;
            TblProduct products = new TblProduct();
            if(id != null && id > 0)
            {
                products = _context.TblProducts.FirstOrDefault(x => x.ProductId == id);
                int UserId = (int) HttpContext.Session.GetInt32("UserId");
                int ShopId = (int) HttpContext.Session.GetInt32("ShopId");
                var stockDetails = _context.TblStocks.FirstOrDefault(x => x.ProductId == id && x.ShopId == ShopId);
                products.ProductQty = stockDetails.Quantity;
                products.ShopPrice = (int) stockDetails.ShopPrice;

                if(products.CompanyId > 0)
                {
                    products.CompanyName = _context.TblCompanies.Where(x => x.CompanyId == products.CompanyId).Select(y => y.CompanyName).FirstOrDefault();
                }
                else
                {
                    products.CompanyName = _context.TblUnregCompanies.Where(x => x.UnregCompanyId == products.UnregCompanyId).Select(y => y.UnregCompanyName).FirstOrDefault();
                }
            }
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> SaveShopProduct([FromForm] TblProduct product)
        {
                int qty = product.ProductQty;
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int shopId = (int)HttpContext.Session.GetInt32("ShopId");
            if(product.ProductId == 0)
            {
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
            else
            {
                await _product.SaveStockAsync(product.ProductId, shopId, qty, product.ShopPrice);
                return Json(new { success = true, message = "Product Stock Update Successfully" });
            }
        }
    }
}
