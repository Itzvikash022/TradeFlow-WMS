using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using WMS_Application.DTO;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WMS_Application.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly dbMain _context;
        public DashboardRepository(dbMain context)
        {
            _context = context;
        }

        public decimal GetTotalSalesThisMonth(int shopId)
        {
            decimal sales = _context.TblOrders
            .Where(o => o.SellerId == shopId &&
                        o.OrderStatus == "Success" &&
                        o.PaymentStatus== "Paid" &&
                        o.OrderDate.Value.Month == DateTime.Now.Month &&
                        o.OrderDate.Value.Year == DateTime.Now.Year)
            .Sum(o => o.TotalAmount);  // or from transaction table

            return sales;
        }

        public decimal GetTotalPurchaseThisMonth(int shopId)
        {
            decimal purchase = _context.TblOrders
            .Where(o => o.BuyerId == shopId &&
                        o.OrderStatus == "Success" &&
                        o.PaymentStatus == "Paid" &&
                        o.OrderDate.Value.Month == DateTime.Now.Month &&
                        o.OrderDate.Value.Year == DateTime.Now.Year)
            .Sum(o => o.TotalAmount);  // or from transaction table

            return purchase;
        }
        
        public int GetTotalOrdersThisMonth(int shopId)
        {
            var orderCount = _context.TblOrders
            .Where(o => (o.SellerId == shopId || o.BuyerId == shopId) &&
                        o.OrderDate.Value.Month == DateTime.Now.Month &&
                        o.OrderDate.Value.Year == DateTime.Now.Year)
            .Count();

            return orderCount;
        }
        
        public int GetOverallPendingOrders(int shopId)
        {
            var pendingOrders = _context.TblOrders
                .Where(o => (o.SellerId == shopId || o.BuyerId == shopId) &&
                            o.OrderStatus == "Pending")
                .Count();

            return pendingOrders;
        }
        
        public int GetBuyers(int shopId)
        {
            var totalBuyers = _context.TblOrders
            .Where(o => o.SellerId == shopId)
            .Select(o => o.BuyerId)
            .Distinct()
            .Count();


            return totalBuyers;
        }
        
        public int GetSellers(int shopId)
        {
            var totalSellers = _context.TblOrders
            .Where(o => o.BuyerId == shopId)
            .Select(o => o.SellerId)
            .Distinct()
            .Count();

            return totalSellers;
        }
        
        public int GetTotalProductQty(int shopId)
        {
            var totalQty = _context.TblStocks
            .Where(s => s.ShopId == shopId) // or CompanyId
            .Sum(s => s.Quantity);


            return totalQty;
        }

        public int GetProfitThisMonth(int shopId)
        {
            var totalProfit = _context.TblOrders
            .Where(x => x.SellerId == shopId && x.OrderStatus == "Success")
            .Sum(y => y.TotalProfit);

            return totalProfit;

        }

        public int GetTotalProductQtyCompany(int companyId)
        {
            var totalQty = _context.TblProducts
            .Where(s => s.CompanyId == companyId && s.IsDeleted == false) // or CompanyId
            .Sum(s => s.ProductQty);


            return totalQty;
        }
        
        public int GetUniqueProductQty(int shopId)
        {
            var uniqueProducts = _context.TblStocks
             .Where(s => s.ShopId == shopId)
             .Select(s => s.ProductId)
             .Distinct()
             .Count();

            return uniqueProducts;
        }
        
        public int GetUniqueProductQtyCompany(int companyId)
        {
            var uniqueProducts = _context.TblProducts
            .Where(s => s.CompanyId == companyId && s.IsDeleted == false) // or CompanyId
            .Select(s => s.ProductId)
            .Distinct()
            .Count();


            return uniqueProducts;
        }

        public int GetEmployeeCountByShop(int adminId)
        {
            return _context.TblUsers.Count(u => u.AdminRef == adminId && u.IsDeleted == false);
        }


        public List<MonthlySalesPurchaseDto> GetMonthlySalesPurchaseData(int year, int shopId)
        {
            var result = new List<MonthlySalesPurchaseDto>();

            for (int month = 1; month <= 12; month++)
            {
                var salesAmount = _context.TblOrders
                    .Where(o => o.SellerId == shopId &&
                                o.OrderDate.HasValue &&
                                o.OrderStatus == "Success" &&
                                o.OrderDate.Value.Year == year &&
                                o.OrderDate.Value.Month == month)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                var purchaseAmount = _context.TblOrders
                    .Where(o => o.BuyerId == shopId &&
                                o.OrderDate.HasValue &&
                                o.OrderStatus == "Success" &&
                                o.OrderDate.Value.Year == year &&
                                o.OrderDate.Value.Month == month)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                result.Add(new MonthlySalesPurchaseDto
                {
                    Month = month,
                    Sales = salesAmount,
                    Purchase = purchaseAmount
                });
            }

            return result;
        }

        public List<TblProduct> RecentlyAddedProductsCompany(int companyId)
        {
            var recentProducts = _context.TblProducts
            .Where(p => p.CompanyId == companyId && p.IsDeleted == false) // or ShopId / CompanyId
            .OrderByDescending(p => p.CreateAt)
            .Take(5)
            .Select(p => new TblProduct{
                ProductName = p.ProductName,
                ProductId = p.ProductId,
                ProductImagePath = p.ProductImagePath,
                PricePerUnit = p.PricePerUnit,
                CreateAt = p.CreateAt
            })
            .ToList();

            return recentProducts;
        }

        public List<TblProduct> RecentlyAddedProductsShop(int shopId)
        {
            var recentProducts = _context.TblStocks
            .Where(s => s.ShopId == shopId && s.Quantity > 0)
            .OrderByDescending(s => s.LastUpdated) // or UpdatedAt, based on your schema
            .Take(5)
            .Select(s => new TblProduct
            {
                ProductId = s.Product.ProductId,
                ProductName = s.Product.ProductName,
                ProductImagePath = s.Product.ProductImagePath,
                PricePerUnit = s.ShopPrice,
                CreateAt = s.LastUpdated // or s.Product.CreateAt if you prefer that
            })
            .ToList();

            return recentProducts;
        }


        public List<TblProduct> GetLowStockProductsForShop(int shopId)
        {
            var result = (from stock in _context.TblStocks
                          join prod in _context.TblProducts on stock.ProductId equals prod.ProductId
                          join comp in _context.TblCompanies on prod.CompanyId equals comp.CompanyId into compJoin
                          from company in compJoin.DefaultIfEmpty()
                          join unreg in _context.TblUnregCompanies on prod.UnregCompanyId equals unreg.UnregCompanyId into unregJoin
                          from unregisteredCompany in unregJoin.DefaultIfEmpty()
                          where stock.ShopId == shopId && stock.Quantity < 10
                          orderby stock.Quantity ascending
                          select new TblProduct
                          {
                              ProductId = prod.ProductId,
                              ProductName = prod.ProductName,
                              ProductQty = stock.Quantity,
                              ProductImagePath = prod.ProductImagePath,
                              Manufacturer = prod.Manufacturer,
                              CompanyName = prod.CompanyId > 0
                                            ? company.CompanyName
                                            : (prod.UnregCompanyId > 0
                                                ? unregisteredCompany.UnregCompanyName
                                                : "N/A"),
                              LastUpdateDate = stock.LastUpdated,
                          }).OrderBy(x => x.ProductQty).ToList();

            return result;

        }

        public List<TblProduct> GetLowStockProductsForCompany(int companyId)
        {
            var result = _context.TblProducts
                .Where(p => p.CompanyId == companyId && p.ProductQty < 10 && p.IsDeleted == false)
                .OrderBy(p => p.ProductQty)
                .Select(p => new TblProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductQty = p.ProductQty,
                    ProductImagePath = p.ProductImagePath,
                    Manufacturer = p.Manufacturer,
                    CompanyName = p.CompanyName,
                    LastUpdateDate = p.CreateAt
                }).OrderBy(x => x.ProductQty).ToList();

            return result;
        }

    }
}
