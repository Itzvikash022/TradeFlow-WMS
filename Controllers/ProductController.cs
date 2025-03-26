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
        private readonly IPermisionHelperRepository _permission;

        public ProductController(ISidebarRepository sidebar, IProductRepository product, dbMain context, IUsersRepository users, IPermisionHelperRepository permission) : base(sidebar)
        {
            _product = product;
            _context = context;
            _users = users;
            _permission = permission;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;

            //checking for Create orders btn
            string OrderPermissionType = _permission.HasAccess("Orders", roleId);
            ViewBag.OrderPermissionType = OrderPermissionType;
            return permissionType;
        }

        [HttpGet]
        [Route("Products")]
        public async Task<IActionResult> Index(int? companyId)
        {
            string permissionType = GetUserPermission("Products");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int Id = 0, shopId = 0;
                TblShop ShopData = new TblShop();
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                if (HttpContext.Session.GetInt32("CompanyId") != null && roleId == 5)
                {
                    Id = (int)HttpContext.Session.GetInt32("CompanyId");
                }
                else
                {
                    Id = companyId ?? 0;
                    int adminId = (int)HttpContext.Session.GetInt32("UserId");
                    if (roleId > 2 && roleId != 5)
                    {
                        adminId = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
                    }
                    ShopData = await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == adminId);
                    shopId = ShopData.ShopId;
                }

                return View(await _product.GetAllProducts(Id, shopId));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public IActionResult SaveShopProduct(int? id)
        {
            string permissionType = GetUserPermission("Products");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                List<TblProductCategory> prodCat = _context.TblProductCategories.Where(x => x.IsActive == true).ToList();
                ViewBag.ProductCategory = prodCat;
                TblProduct products = new TblProduct();
                if (id != null && id > 0)
                {
                    products = _context.TblProducts.FirstOrDefault(x => x.ProductId == id);
                    int UserId = (int)HttpContext.Session.GetInt32("UserId");
                    int ShopId = (int)HttpContext.Session.GetInt32("ShopId");
                    var stockDetails = _context.TblStocks.FirstOrDefault(x => x.ProductId == id && x.ShopId == ShopId);
                    products.ProductQty = stockDetails.Quantity;
                    products.ShopPrice = (int)stockDetails.ShopPrice;

                    if (products.CompanyId > 0)
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
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> SaveShopProduct([FromForm] TblProduct product)
        {
            int qty = product.ProductQty;
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int shopId = (int)HttpContext.Session.GetInt32("ShopId");
            if (product.ProductId == 0)
            {
                int UnregCompanyId = await _product.SaveUnregCompanyAsync(product.CompanyName, userId);
                product.UnregCompanyId = UnregCompanyId;
                var result = await _product.SaveShopProductAsync(product, userId, shopId);

                if (((dynamic)result).success)
                {
                    int newProductId = ((dynamic)result).productId;

                    await _product.SaveStockAsync(newProductId, shopId, qty, product.ShopPrice);
                }
                return Json(result);
            }
            else
            {
                await _product.SaveStockAsync(product.ProductId, shopId, qty, product.ShopPrice);
                return Json(new { success = true, message = "Product Stock Update Successfully" });
            }
        }


        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            if (id == 0)  // Handle the case if the ID is invalid
            {
                return Json(new { success = false, message = "Invalid Product ID." });
            }
            int userRoldId = (int)HttpContext.Session.GetInt32("UserRoleId");
            // Try deleting the admin from the database or perform your logic here
            try
            {
                var product = _context.TblProducts.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product Not Found." });
                }
                if (userRoldId == 5)
                {
                    product.IsDeleted = true;
                    _context.TblProducts.Update(product);
                }
                else
                {
                    int shopId = (int)HttpContext.Session.GetInt32("ShopId");

                    var stock = _context.TblStocks.Where(x => x.ShopId == shopId && x.ProductId == product.ProductId).FirstOrDefault();
                    _context.TblStocks.Remove(stock);
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the deletion
                Console.WriteLine("Error deleting Product: " + ex.Message);
                return Json(new { success = false, message = "Error deleting Product." });
            }
        }

        public async Task<IActionResult> Products()
        {
            string permissionType = GetUserPermission("Products");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddProducts(int? id)
        {
            string permissionType = GetUserPermission("Products");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                List<TblProductCategory> prodCat = _context.TblProductCategories.Where(x => x.IsActive == true).ToList();
                ViewBag.ProductCategory = prodCat;
                TblProduct model = new TblProduct();
                if (id > 0)
                {
                    model = await _context.TblProducts.Where(x => x.ProductId == id).FirstOrDefaultAsync();
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }

        }
    }
}
