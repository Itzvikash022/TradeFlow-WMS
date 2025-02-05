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
            if (companyId > 0)
            {
                // Fetch products for a specific company
                var products = from product in _context.TblProducts
                               join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                               join stock in _context.TblStocks on product.ProductId equals stock.ProductId into stockJoin
                               from stock in stockJoin.DefaultIfEmpty() // Left join to include all products
                               where product.CompanyId == companyId
                               select new TblProduct
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   Category = product.Category,
                                   PricePerUnit = product.PricePerUnit,
                                   CreateAt = product.CreateAt,
                                   LastUpdateDate = product.LastUpdateDate,
                                   Manufacturer = product.Manufacturer,
                                   CompanyName = company.CompanyName,
                                   ProductImagePath = product.ProductImagePath,
                                   ProductQty = stock != null ? stock.Quantity : 0 // Show 0 if no stock found
                               };

                return products.ToList();

            }
            else
            {
                var products = from stock in _context.TblStocks
                               join product in _context.TblProducts on stock.ProductId equals product.ProductId
                               join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                               where stock.ShopId == shopId
                               select new TblProduct
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   Category = product.Category,
                                   PricePerUnit = product.PricePerUnit,
                                   CreateAt = product.CreateAt,
                                   LastUpdateDate = product.LastUpdateDate,
                                   Manufacturer = product.Manufacturer,
                                   ProductQty = stock.Quantity,
                                   CompanyName = company.CompanyName,
                                   ProductImagePath = product.ProductImagePath
                               };

                return products.ToList();
            }
        }

    }
}
