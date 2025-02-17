using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using WMS_Application.DTO;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace WMS_Application.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly dbMain _context;
        public OrdersRepository(dbMain context)
        {
            _context = context;
        }

        public async Task<List<TblOrder>> GetAllOrders()
        {
            var orders = await (from o in _context.TblOrders

                                    // Left Join for Seller Shop
                                join sShop in _context.TblShops on o.SellerId equals sShop.ShopId into sShopJoin
                                from sShop in sShopJoin.DefaultIfEmpty()

                                    // Left Join for Buyer Shop
                                join bShop in _context.TblShops on o.BuyerId equals bShop.ShopId into bShopJoin
                                from bShop in bShopJoin.DefaultIfEmpty()

                                    // Left Join for Seller Company
                                join comp in _context.TblCompanies on o.SellerId equals comp.CompanyId into compJoin
                                from comp in compJoin.DefaultIfEmpty()

                                    // Left Join for Buyer Customer
                                join cust in _context.TblCustomers on o.BuyerId equals cust.CustomerId into custJoin
                                from cust in custJoin.DefaultIfEmpty()

                                select new TblOrder
                                {
                                    OrderId = o.OrderId,
                                    OrderDate = o.OrderDate,
                                    OrderType = o.OrderType,

                                    // **✅ Fix: Correctly fetching Seller Name**
                                    SellerName = o.OrderType == "ShopToShopBuy" || o.OrderType == "ShopToShopSell" ? sShop.ShopName :
                                                 o.OrderType == "CompanyToShop" ? comp.CompanyName :
                                                 o.OrderType == "ShopToCustomer" ? sShop.ShopName :
                                                 "Unknown Seller",

                                    // **✅ Fix: Correctly fetching Buyer Name**
                                    BuyerName = o.OrderType == "ShopToShopBuy" || o.OrderType == "ShopToShopSell" ? bShop.ShopName :
                                                o.OrderType == "ShopToCustomer" && cust != null ? cust.CustomerName :
                                                bShop != null ? bShop.ShopName :
                                                "Unknown Buyer",

                                    TotalAmount = o.TotalAmount,
                                    Remarks = o.Remarks,
                                    OrderStatus = o.OrderStatus,
                                    TotalQty = o.TotalQty
                                })
                     .OrderByDescending(o => o.OrderDate).AsNoTracking()
                     .ToListAsync();

            return orders;
        }


        public async Task<List<ProductS2SBuyDto>> GetAllProducts(int companyId, int shopId)
        {

            if (companyId > 0)
            {
                // Fetch products for a specific company
                var products = from product in _context.TblProducts
                               join company in _context.TblCompanies on product.CompanyId equals company.CompanyId
                               join stock in _context.TblStocks on product.ProductId equals stock.ProductId into stockJoin
                               from stock in stockJoin.DefaultIfEmpty() // Left join to include all products
                               where product.CompanyId == companyId
                               select new ProductS2SBuyDto
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   Category = product.Category,
                                   PricePerUnit = product.PricePerUnit,
                                   SellerShopId = 0,
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
                               select new ProductS2SBuyDto
                               {
                                   ProductId = product.ProductId,
                                   ProductName = product.ProductName,
                                   Category = product.Category,
                                   PricePerUnit = product.PricePerUnit,
                                   ProductQty = stock.Quantity,
                                   SellerShopId = shopId,
                                   CompanyName = company.CompanyName,
                                   ProductImagePath = product.ProductImagePath
                               };

                return products.ToList();
            }
        }


        public List<ProductS2SBuyDto> GetProductsC2S(string? productName, string? category, string? company)
        {
            var query = _context.TblProducts.AsQueryable();

            // Fetch company data only if a company name is provided
            TblCompany companyData = null;
            if (!string.IsNullOrEmpty(company))
            {
                companyData = _context.TblCompanies.FirstOrDefault(x => x.CompanyName == company);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(p => p.ProductName.Contains(productName));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (companyData != null) // ✅ Check if company exists before filtering
            {
                query = query.Where(p => p.CompanyId == companyData.CompanyId);
            }

            var result = query.ToList();

            //foreach (var product in result)
            //{
            //    var companyInfo = _context.TblCompanies
            //        .FirstOrDefault(c => c.CompanyId == product.CompanyId);

            //    product.CompanyName = companyInfo?.CompanyName; // ✅ Assign company name
            //}

            // Convert to DTO and assign values
            var productsDto = result.Select(product => new ProductS2SBuyDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Category = product.Category,
                CompanyId = product.CompanyId,
                ProductImagePath = product.ProductImagePath,
                ProductQty = product.ProductQty,
                PricePerUnit = product.PricePerUnit,
                CompanyName = _context.TblCompanies
                    .Where(c => c.CompanyId == product.CompanyId)
                    .Select(c => c.CompanyName)
                    .FirstOrDefault(),
                SellerShopId = null // 👈 Always null for C2S

            }).ToList();
            return productsDto;
        }


        public List<ProductS2SBuyDto> GetProductsS2SBuy(string? productName, string? category, int? shop, int userId)
        {
            var loggedInShop = _context.TblShops
                .Where(x => x.AdminId == userId)
                .Select(x => x.ShopId)
                .FirstOrDefault();

            var query = _context.TblStocks.AsQueryable();

            // Exclude logged-in shop's products
            if (userId > 0)
            {
                query = query.Where(s => s.ShopId != loggedInShop);
            }

            if (shop != null)
            {
                query = query.Where(s => s.ShopId != shop);
            }

            // Apply additional filters
            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(s => s.Product.ProductName.Contains(productName));
            }
                
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.Product.Category == category);
            }

            // Fetch only necessary fields using DTO, ensuring multiple shops can list the same product separately
            var result = query
                .Select(s => new ProductS2SBuyDto
                {
                    ProductId = s.Product.ProductId,
                    ProductName = s.Product.ProductName,
                    Category = s.Product.Category,
                    PricePerUnit = s.Product.PricePerUnit,
                    ProductImagePath = s.Product.ProductImagePath,
                    CompanyName = s.Shop.ShopName,  // Fetching shop name correctly per product-shop entry
                    ProductQty = s.Quantity,         // No sum, so each shop's product is listed separately
                    SellerShopId = s.Shop.ShopId,
                    CompanyId = s.Product.CompanyId
                })
                .ToList();

            return result;
        }


        public List<TblShop> GetShopDetails(int currentAdminId)
        {
            
            var shops = _context.TblShops
                .Where(s => s.AdminId != currentAdminId) // Exclude current shop
                .ToList();

            return shops;
        }

        public ShopDetailsDTO GetShopKeepersDetails(int selectedShopId)
            {
                var selectedShopDetails = _context.TblShops
                    .Where(s => s.ShopId == selectedShopId)
                    .Select(s => new ShopDetailsDTO
                    {
                        OwnerName = _context.TblUsers
                            .Where(u => u.UserId == s.AdminId)
                            .Select(u => u.Username)
                            .FirstOrDefault(),
                        ShopAddress = s.Address
                    })
                    .FirstOrDefault();

                return selectedShopDetails;
            }


        //Inserting Orders data in tblOrders
        // Generalized function to handle both C2S and S2Cst orders
        public async Task<int> CreateOrderAsync(int sellerId, int buyerId, string orderType, int totalQty, decimal totalAmount, List<ProductDto> products, string status)
        {
            if (products == null || products.Count == 0)
                throw new ArgumentException("No products provided for the order.");

            // Create new order
            var newOrder = new TblOrder
            {
                OrderDate = DateTime.Now,
                SellerId = sellerId, // Can be companyId (C2S) or shopId (S2Cst)
                BuyerId = buyerId,   // Can be shopId (C2S) or customerId (S2Cst)
                OrderType = orderType,
                TotalAmount = totalAmount,
                OrderStatus = status,
                Remarks = "ok",
                TotalQty = totalQty
            };

            _context.TblOrders.Add(newOrder);
            await _context.SaveChangesAsync();
            int orderId = newOrder.OrderId;

            // Insert order details
            var orderDetails = products.Select(p => new TblOrderDetail
            {
                OrderId = orderId,
                ProductId = p.ProductID,
                Quantity = p.qty,
                PricePerUnit = p.PricePerUnit, // Uses either company or shop price
                Amount = p.PricePerUnit * p.qty
            }).ToList();

            _context.TblOrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();

            // Update stock based on order type
            await UpdateStockAsync(orderType, products, sellerId, buyerId);

            return orderId;
        }

        // Generalized stock update function
        public async Task UpdateStockAsync(string orderType, List<ProductDto> products, int sellerId, int buyerId)
        {
            foreach (var product in products)
            {
                if (orderType == "CompanyToShop")
                {
                    // Reduce stock from tblProducts (Company stock)
                    var companyProduct = await _context.TblProducts
                        .FirstOrDefaultAsync(p => p.ProductId == product.ProductID);

                    if (companyProduct != null && companyProduct.ProductQty >= product.qty)
                    {
                        companyProduct.ProductQty -= product.qty;
                    }
                    else
                    {
                        throw new Exception($"Insufficient stock for product ID {product.ProductID}");
                    }

                    // Increase stock in the buyer's shop (tblStock)
                    var shopStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == buyerId && s.ShopPrice == product.PricePerUnit);

                    if (shopStock != null)
                    {
                        shopStock.Quantity += product.qty;
                    }
                    else
                    {
                        _context.TblStocks.Add(new TblStock
                        {
                            ProductId = product.ProductID,
                            ShopId = buyerId,
                            Quantity = product.qty,
                            ShopPrice = product.PricePerUnit
                        });
                    }
                }
                else if (orderType == "ShopToCustomer")
                {
                    // Reduce stock from the seller's shop (tblStock)
                    var shopStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == sellerId);

                    if (shopStock != null && shopStock.Quantity >= product.qty)
                    {
                        shopStock.Quantity -= product.qty;
                    }
                    else
                    {
                        throw new Exception($"Insufficient stock for product ID {product.ProductID} in shop.");
                    }
                }
                else if (orderType == "ShopToShopBuy" || orderType == "ShopToShopSell")
                {
                    // Reduce stock from the seller's shop (Shop 1)
                    var sellerStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == sellerId);

                    if (sellerStock != null && sellerStock.Quantity >= product.qty)
                    {
                        sellerStock.Quantity -= product.qty;
                    }
                    else
                    {
                        throw new Exception($"Insufficient stock for product ID {product.ProductID} in seller shop.");
                    }

                    // Increase stock in the buyer's shop (Shop 2)
                    var buyerStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == buyerId && s.ShopPrice == product.PricePerUnit);

                    if (buyerStock != null)
                    {
                        buyerStock.Quantity += product.qty;
                    }
                    else
                    {
                        _context.TblStocks.Add(new TblStock
                        {
                            ProductId = product.ProductID,
                            ShopId = buyerId,
                            Quantity = product.qty,
                            ShopPrice = product.PricePerUnit
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }





    }
}
