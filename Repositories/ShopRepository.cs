using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly dbMain _context;
        public ShopRepository(dbMain context)
        {
            _context = context;
        }   

        public async Task<List<TblShop>> GetAllShops()
        {
            var query = (from shop in _context.TblShops
                        join user in _context.TblUsers on  shop.AdminId equals user.UserId
                        where shop.IsDeleted == false && shop.IsActive == true
                        select new TblShop
                        {
                            ShopId = shop.ShopId,
                            AdminId = shop.AdminId,
                            ShopName = shop.ShopName,
                            ShopOwner = user.FirstName +" "+ user.LastName,
                            CreatedAt = shop.CreatedAt,
                            State = shop.State,
                            City = shop.City,
                            Pincode = shop.Pincode,
                            Address = shop.Address,
                            ShopImagePath = shop.ShopImagePath,
                            StartTime = shop.StartTime,
                            ClosingTime = shop.ClosingTime,
                        }).OrderByDescending(x => x.CreatedAt);
            return await query.ToListAsync();
        }

     public List<TblShop> GetShopReports(int userId)
        {
            List<TblShop> shopData = _context.TblShops.Where(x => x.IsDeleted == false && x.AdminId != userId).OrderByDescending(x=> x.CreatedAt).ToList();
            foreach (var shop in shopData)
            {
                shop.OrderCount = _context.TblOrders.Where(x => x.SellerId == shop.ShopId || x.BuyerId == shop.ShopId).Count();
                shop.ShopOwner = _context.TblUsers.Where(x => x.UserId == shop.AdminId).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
            }
            return shopData;
        }
    }
}
