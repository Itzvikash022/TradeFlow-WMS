using Humanizer;
using MailKit.Search;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Tls;
using System;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
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
            List<TblOrder> orders =  _context.TblOrders
                .Select(o => new TblOrder
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    OrderType = o.OrderType,
                    SellerName = o.OrderType == "ShopToShop"
                        ? _context.TblShops.FirstOrDefault(s => s.ShopId == o.SellerId).ShopName
                        : o.OrderType == "SupplierToShop"
                            ? _context.TblCompanies.FirstOrDefault(c => c.CompanyId == o.SellerId).CompanyName
                            : "Unknown Seller",
                    BuyerName = o.OrderType == "ShopToShop"
                        ? _context.TblShops.FirstOrDefault(s => s.ShopId == o.BuyerId).ShopName
                        : o.OrderType == "ShopToCustomer"
                            ? _context.TblShops.FirstOrDefault(s => s.ShopId == o.SellerId).ShopName
                            : "Unknown Buyer",
                    TotalAmount = o.TotalAmount,
                    Remarks = o.Remarks
                })
                .OrderByDescending(o => o.OrderDate)
                .ToList();  

            return orders;
        }

    }
}
