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

        public async Task<List<TblProduct>> GetAllProducts()
        {
            var products = from product in _context.TblProducts
                            join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                            select new TblProduct
                            {
                                ProductId = product.ProductId,
                                ProductName = product.ProductName,
                                Category = product.Category,
                                PricePerUnit = product.PricePerUnit,
                                CreateAt = product.CreateAt,
                                LastUpdateDate = product.LastUpdateDate,
                                Manufacturer = product.Manufacturer,
                                CompanyName = company.CompanyName
                            };
            return products.ToList();
        }
    }
}
