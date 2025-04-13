using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Data;
using System.IO.Compression;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductRepository _product;
        private readonly dbMain _context;
        private readonly IUsersRepository _users;
        private readonly IExportServiceRepository _export;
        private readonly IPermisionHelperRepository _permission;
        private readonly IImportServiceRepository _import;

        public ProductController(ISidebarRepository sidebar, IProductRepository product, dbMain context, IUsersRepository users, IPermisionHelperRepository permission, IExportServiceRepository export, IImportServiceRepository import) : base(sidebar)
        {
            _product = product;
            _context = context;
            _users = users;
            _permission = permission;
            _export = export;
            _import = import;
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

        public IActionResult DownloadSampleFile()
        {
            var fileBytes = _import.GenerateSampleStockExcel(false);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleFile.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSampleFile(IFormFile excelFile, IFormFile imageZip)
        {
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (excelFile == null || imageZip == null)
                return BadRequest("Please upload both Excel file and Image ZIP folder.");

            if (!imageZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .zip files are allowed for image upload." });
            }
            // Extract ZIP file
            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ProductUploads");
            Directory.CreateDirectory(imageFolderPath);

            using (var zipStream = new MemoryStream())
            {
                await imageZip.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string extension = Path.GetExtension(entry.Name).ToLower();

                        // ❌ Reject non-image files
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, message = "only image files are allowed inside the zip" });
                        }

                        // ✅ Move valid images
                        var filePath = Path.Combine(imageFolderPath, entry.Name);
                        if (!System.IO.File.Exists(filePath))
                        {
                            entry.ExtractToFile(filePath);
                        }
                    }
                }
            }



            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "File is empty." });
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Invalid file format. Only .xlsx files are allowed." });
            }

            try
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int shopId = (int)HttpContext.Session.GetInt32("ShopId");
                var fileBytes = await _import.ShopProcessStockImport(excelFile, userId, shopId);

                // Convert to Base64 to store temporarily
                string fileBase64 = Convert.ToBase64String(fileBytes);

                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully!",
                    fileData = fileBase64,
                    fileName = "OutputOfSampleFile.xlsx"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        public async Task<IActionResult> ExportProductList(int? companyId)
        {
            int Id = 0, shopId = 0;
            var dataTable = new DataTable("Products");

            TblShop ShopData = new TblShop();
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if (HttpContext.Session.GetInt32("CompanyId") != null && roleId == 5)
            {
                Id = (int)HttpContext.Session.GetInt32("CompanyId");
                dataTable.Columns.AddRange(new DataColumn[]
                   {
                        new DataColumn("Product Name"),
                        new DataColumn("Category"),
                        new DataColumn("Price Per Unit"),
                        new DataColumn("Manufacturer"),
                        new DataColumn("Stock Quantity"),
                   });
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

                dataTable.Columns.AddRange(new DataColumn[]
                   {
                        new DataColumn("Product Name"),
                        new DataColumn("Category"),
                        new DataColumn("CompanyName"),
                        new DataColumn("Price Per Unit"),
                        new DataColumn("Manufacturer"),
                        new DataColumn("Quantity In Shop"),
                        new DataColumn("Shop Price"),
                   });
            }

            var products = await _product.GetAllProducts(Id, shopId);


            if(HttpContext.Session.GetInt32("CompanyId") != null && roleId == 5)
            {
                foreach (var product in products)
                {
                    dataTable.Rows.Add(product.CompanyName, product.ProdCategory, product.PricePerUnit, product.Manufacturer, product.ProductQty);
                }
            }
            else
            {
                foreach (var product in products)
                {
                    dataTable.Rows.Add(product.ProductName, product.ProdCategory, product.CompanyName, product.PricePerUnit, product.Manufacturer, product.ProductQty, product.ShopPrice);
                }
            }

            var fileBytes = _export.ExportToExcel(dataTable, "ProductsList");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductsList.xlsx");
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
                    TempData["shopProd-toast"] = "Product Stock Added Successfully";
                    TempData["shopProd-toastType"] = "success";
                }
                return Json(result);
            }
            else
            {
                await _product.SaveStockAsync(product.ProductId, shopId, qty, product.ShopPrice);
                TempData["shopProd-toast"] = "Product Stock Updated Successfully";
                TempData["shopProd-toastType"] = "success";
                return Json(new { success = true, message = "Product Stock Updated Successfully" });
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
