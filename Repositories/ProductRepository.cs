using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WMS_Application.DTO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly dbMain _context;
        public ProductRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<List<TblProduct>> GetAllProducts(int companyId, int shopId)
        {
            if (companyId > 0 && shopId > 0)
            {
                // Fetch products for a specific company
                var products = (from product in _context.TblProducts
                               join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                               join stock in _context.TblStocks on product.ProductId equals stock.ProductId into stockJoin
                               from stock in stockJoin.DefaultIfEmpty() // Left join to include all products
                               where company.CompanyId == companyId && product.IsDeleted == false
                               select new TblProduct
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   ProdCategory = _context.TblProductCategories.Where(c => c.ProdCatId == product.Category)
                                    .Select(c => c.ProductCategory)
                                    .FirstOrDefault(),
                                   PricePerUnit = product.PricePerUnit,
                                   CreateAt = product.CreateAt,
                                   LastUpdateDate = product.LastUpdateDate,
                                   Manufacturer = product.Manufacturer,
                                   CompanyName = company.CompanyName,
                                   ProductImagePath = product.ProductImagePath,
                                   ProductQty = stock != null ? stock.Quantity : 0 // Show 0 if no stock found
                               }).OrderByDescending(p => p.LastUpdateDate);

                return products.ToList();

            }
            else if (companyId > 0 && shopId == 0)
            {
                // Fetch products for my company
                var products = (from product in _context.TblProducts
                               join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                               where product.CompanyId == companyId && product.ProductQty > 0 && product.IsDeleted == false
                               select new TblProduct
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   ProdCategory = _context.TblProductCategories.Where(c => c.ProdCatId == product.Category)
                    .Select(c => c.ProductCategory)
                    .FirstOrDefault(),
                                   PricePerUnit = product.PricePerUnit,
                                   CreateAt = product.CreateAt,
                                   LastUpdateDate = product.LastUpdateDate,
                                   Manufacturer = product.Manufacturer,
                                   CompanyName = company.CompanyName,
                                   ProductImagePath = product.ProductImagePath,
                                   ProductQty = product.ProductQty,
                               }).OrderByDescending(p => p.LastUpdateDate);

                return products.ToList();
            }
            else
            {
                var products = (from stock in _context.TblStocks
                               join product in _context.TblProducts
                               on stock.ProductId equals product.ProductId

                               // Left Join with Registered Companies
                               join company in _context.TblCompanies
                               on product.CompanyId equals company.CompanyId into companyGroup
                               from company in companyGroup.DefaultIfEmpty()

                                   // Left Join with Unregistered Companies
                               join unregCompany in _context.TblUnregCompanies
                               on product.UnregCompanyId equals unregCompany.UnregCompanyId into unregCompanyGroup
                               from unregCompany in unregCompanyGroup.DefaultIfEmpty()

                               where stock.ShopId == shopId && stock.Quantity > 0   
                               select new TblProduct
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   ProdCategory = _context.TblProductCategories.Where(c => c.ProdCatId == product.Category).Select(c => c.ProductCategory).FirstOrDefault(),
                                   PricePerUnit = product.PricePerUnit,
                                   CreateAt = product.CreateAt,
                                   LastUpdateDate = stock.LastUpdated,
                                   Manufacturer = product.Manufacturer,
                                   ProductQty = stock.Quantity,
                                   ShopPrice = (int)stock.ShopPrice,
                                   CompanyName = company != null ? company.CompanyName : unregCompany.UnregCompanyName, // Take company name from whichever is available
                                   ProductImagePath = product.ProductImagePath
                               }).OrderByDescending(p => p.LastUpdateDate);

                return products.ToList();

            }
        }

        public async Task<object> SaveShopProductAsync(TblProduct product, int UserId, int ShopId)
        {
            try
            {
                product.CompanyId = 0;
                var UpdatedProduct = await _context.TblProducts.FirstOrDefaultAsync(u => u.ProductId == product.ProductId);
                string imgPath = "";

                if (product.ProductImage != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(product.ProductImage.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return new { success = false, message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." };
                    }

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ProductUploads");
                    Directory.CreateDirectory(uploadsFolder); // Ensures folder exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ProductImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ProductImage.CopyToAsync(stream);
                    }

                    imgPath = "\\ProductUploads\\" + uniqueFileName;
                }

                product.ProductQty = 0;

                string msg;
                int productId;

                if (UpdatedProduct != null)  // Updating existing product
                {
                    UpdatedProduct.CompanyId = product.CompanyId;
                    UpdatedProduct.UnregCompanyId = product.UnregCompanyId;
                    UpdatedProduct.ProductName = product.ProductName;
                    UpdatedProduct.Category = product.Category;
                    UpdatedProduct.PricePerUnit = product.PricePerUnit;
                    UpdatedProduct.Manufacturer = product.Manufacturer;
                    UpdatedProduct.LastUpdateDate = DateTime.Now;

                    if (product.ProductImage != null)
                    {
                        UpdatedProduct.ProductImagePath = imgPath;
                    }

                    _context.TblProducts.Update(UpdatedProduct);
                    await _context.SaveChangesAsync(); // ✅ Ensure changes are saved

                    productId = UpdatedProduct.ProductId;
                    msg = "Product data has been updated successfully";
                }
                else  // Adding new product
                {
                    product.ProductImage = null;
                    product.ProductImagePath = imgPath;
                    product.LastUpdateDate = DateTime.Now;

                    var addedProduct = await _context.TblProducts.AddAsync(product);
                    await _context.SaveChangesAsync();  // ✅ Ensure ProductId is assigned after saving

                    productId = product.ProductId;  // ✅ Now it will have the correct ID
                    msg = "New Product has been added successfully";
                }

                return new { success = true, message = msg, productId };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        public async Task<int> SaveUnregCompanyAsync(string CompanyName, int AddedBy)
        {
            var existingCompany = await _context.TblUnregCompanies
                .FirstOrDefaultAsync(c => c.UnregCompanyName == CompanyName);

            if (existingCompany != null)
            {
                return existingCompany.UnregCompanyId; // Return existing ID
            }

            TblUnregCompany unregCompany = new TblUnregCompany
            {
                UnregCompanyName = CompanyName,
                AddedBy = AddedBy
            };

            _context.TblUnregCompanies.Add(unregCompany);
            await _context.SaveChangesAsync(); // ✅ Ensure it saves before returning ID

            return unregCompany.UnregCompanyId; // ✅ Now ID is correctly assigned
        }


        public async Task SaveStockAsync(int ProductId, int ShopId, int Quantity, int shopPrice)
        {
            var existingStock = await _context.TblStocks
                .FirstOrDefaultAsync(s => s.ProductId == ProductId && s.ShopId == ShopId);
            if (existingStock != null)
            {
                existingStock.Quantity = Quantity;
                existingStock.ShopPrice = shopPrice; 
                _context.TblStocks.Update(existingStock);
            }
            else
            {
                TblStock stock = new TblStock
                {
                    ProductId = ProductId,
                    ShopId = ShopId,
                    Quantity = Quantity,
                    ShopPrice = shopPrice
                };
                _context.TblStocks.Add(stock);
            }
            await _context.SaveChangesAsync();
        }

        public List<ProductReportsDTO> GetProductsReports(int userId, int roleId)
        {
            var products = new List<ProductReportsDTO>();

            if (roleId == 5) // If Company User
            {
                products = _context.TblProducts
                    .Where(p => p.CompanyId == userId)
                    .Select(p => new ProductReportsDTO
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        BoughtOn = p.CreateAt,
                        PricePerUnit = (int)p.PricePerUnit,
                        Manufacturer = p.Manufacturer,
                        CompanyName = _context.TblCompanies
                            .Where(c => c.CompanyId == p.CompanyId)
                            .Select(c => c.CompanyName)
                            .FirstOrDefault(),
                        ProductImagePath = p.ProductImagePath,
                        ProductQty = p.ProductQty, // No stock data for company-level users
                        SalesCount = _context.TblOrderDetails
                            .Where(od => od.ProductId == p.ProductId)
                            .Sum(od => (int?)od.Quantity) ?? 0
                    }).OrderByDescending(x => x.BoughtOn)
                    .ToList();
            }
            else // If Admin, Fetch Stock Data
            {
                products = (from stock in _context.TblStocks
                            join product in _context.TblProducts on stock.ProductId equals product.ProductId
                            where stock.ShopId == userId
                            select new ProductReportsDTO
                            {
                                ProductId = product.ProductId,
                                ProductName = product.ProductName,
                                CompanyName = _context.TblCompanies
                                    .Where(c => c.CompanyId == product.CompanyId)
                                    .Select(c => c.CompanyName)
                                    .FirstOrDefault(),
                                PricePerUnit = (int)product.PricePerUnit,
                                ProductImagePath = product.ProductImagePath,
                                UnregCompanyName = _context.TblUnregCompanies
                                    .Where(uc => uc.UnregCompanyId == product.UnregCompanyId)
                                    .Select(uc => uc.UnregCompanyName)
                                    .FirstOrDefault(),
                                Manufacturer = product.Manufacturer,
                                ProductQty = stock.Quantity, // Get stock quantity for Admin
                                BoughtOn = stock.LastUpdated,
                                ShopPrice = (int)stock.ShopPrice,
                                SalesCount = _context.TblOrderDetails
                                    .Where(od => od.ProductId == product.ProductId)
                                    .Sum(od => (int?)od.Quantity) ?? 0
                            }).OrderByDescending(x => x.BoughtOn).ToList();

            }

            return products;
        }



    }
}
