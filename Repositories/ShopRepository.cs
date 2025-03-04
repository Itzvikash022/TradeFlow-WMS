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
            var query = from shop in _context.TblShops
                        join user in _context.TblUsers on  shop.AdminId equals user.UserId
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
                        };
            return await query.ToListAsync();
        }

    }
}
