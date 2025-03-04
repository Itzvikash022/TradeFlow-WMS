using WMS_Application.DTO;
using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        Task<List<TblOrder>> GetAllOrders(int loggedInShopId);
        List<ProductS2SBuyDto> GetProductsC2S(string? productName, string? category, string? company);
        List<ProductS2SBuyDto> GetProductsS2SBuy(string? productName, string? category, int? shop, int userId);
        List<TblShop> GetShopDetails(int currentAdminId);
        ShopDetailsDTO GetShopKeepersDetails(int selectedShopId);
        Task<int> CreateOrderAsync(int companyId, int shopId, string orderType, int totalQty, decimal totalAmount, List<ProductDto> products, string status, string paymentStatus);
        Task UpdateStockAsync(int orderId, List<ProductDto> products);
        Task<List<ProductS2SBuyDto>> GetAllProducts(int companyId, int shopId);
        Task<object> AddTransactionInfo(TblTransaction transaction);
        OrderDetailsDTO GetOrderDetails(int orderId);
        Task<List<TblOrder>> GetAllCompanyOrders(int companyId);

    }
}
