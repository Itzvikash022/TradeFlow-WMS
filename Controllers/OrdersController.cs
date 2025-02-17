using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using WMS_Application.Models;
using WMS_Application.DTO;
using WMS_Application.Repositories.Interfaces;
using MimeKit.Tnef;

namespace WMS_Application.Controllers
{
    public class OrdersController : BaseController
    {
        private IOrdersRepository _orders;
        private dbMain _context;
        private IProductRepository _product;
        private ICustomerRepository _customer;
        public OrdersController(ISidebarRepository sidebar, IOrdersRepository orders, dbMain context, IProductRepository product, ICustomerRepository customer) : base(sidebar)
        {
            _orders = orders;
            _context = context;
            _product = product;
            _customer = customer;
        }
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            return View(await _orders.GetAllOrders());
        }

        public IActionResult CreateOrder()
        {
            int currentAdminId = (int)HttpContext.Session.GetInt32("UserId");
            var shops = _orders.GetShopDetails(currentAdminId);
            ViewBag.ShopData = shops;
            return View();
        }

        public IActionResult GetFilteredProducts(string? productName, string? category, string? company)
        {
            var result = _orders.GetProductsC2S(productName, category, company);
            if(result == null)
            {
                return Ok(new { text = "No products available." });
            }
            string msg = "Products found";
            return Ok(new
            {
                msg,
                result
            });

        }

        public IActionResult GetFilteredProductsShop(string? productName, string? category, int? shop)
        {
            int userId = (int) HttpContext.Session.GetInt32("UserId");
            var result = _orders.GetProductsS2SBuy(productName, category, shop, userId);
            if (result == null)
            {
                return Ok(new { text = "No products available." });
            }
            string msg = "Products found";
            return Ok(new
            {
                msg,
                result
            });
        }

        public async Task<IActionResult> GetProductsShopSell(int selectedShopId)
        {
            var ShopKeeperDetails = _orders.GetShopKeepersDetails(selectedShopId);

            //Fetching product
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            var result = _orders.GetAllProducts(0, ShopData.ShopId);
            if (result == null)
            {
                return Ok(new { text = "No products available." });
            }

            return Ok(new
            {
                result
            });
        }

        public async Task<IActionResult> GetProductsS2C(string customerName, string customerEmail, string customerPhone)
        {
            TblCustomer customer = new TblCustomer()
            {
                CustomerName = customerName,
                Email = customerEmail,
                Contact = customerPhone
            };

            string msg;
            var existingCustomer = _context.TblCustomers.FirstOrDefault(c => c.Email == customer.Email || c.Contact == customer.Contact);

            int CustomerId = 0;
            if (existingCustomer != null)
            {
                CustomerId = existingCustomer.CustomerId;
                msg = "This is a returning Customer";
            }
            else
            {
                await _customer.SaveCustomer(customer);
                CustomerId = customer.CustomerId;
                msg = "New Customer added successfully";
            }

            //Fetching product
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            var result = _orders.GetAllProducts(0, ShopData.ShopId);
            if(result == null)
            {
                return Ok(new { text = "No products available." });
            }

            return Ok(new{
                msg, result, customerId = CustomerId
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetShopKeepersDetails(int selectedShopId)
        {
            var shopData = _orders.GetShopKeepersDetails(selectedShopId);
            return Json(shopData);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyShopProducts()
        {
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            var result = _orders.GetAllProducts(0, ShopData.ShopId);
            if (result == null)
            {
                return Ok(new { text = "No products available." });
            }

            return Json(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderDto)
        {
            if (orderDto == null || orderDto.Products == null || !orderDto.Products.Any())
            {
                return BadRequest("Invalid order data.");
            }

            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            try
            {
                int orderId = 0;
                if(orderDto.CustomerId > 0)
                {
                    // Call service function with proper parameters
                    orderId = await _orders.CreateOrderAsync(
                        orderDto.ShopId,
                        orderDto.CustomerId,
                        "ShopToCustomer",
                        orderDto.TotalQty,
                        orderDto.TotalAmount,
                        orderDto.Products,
                        "Success"
                    );
                }
                else if (orderDto.SellerShopId > 0 && orderDto.SellerShopId != ShopData.ShopId) // Shop-to-Shop Buying (S2SB)
                {
                    orderId = await _orders.CreateOrderAsync(
                        orderDto.SellerShopId,      // Seller Shop
                        orderDto.ShopId, // Buyer Shop
                        "ShopToShopBuy",
                        orderDto.TotalQty,
                        orderDto.TotalAmount,
                        orderDto.Products,
                        "Pending"
                    );
                }
                else if (orderDto.SellerShopId > 0 && orderDto.SellerShopId == ShopData.ShopId) // Shop-to-Shop Selling (S2Ss)
                {
                    orderId = await _orders.CreateOrderAsync(
                        orderDto.SellerShopId,      // Seller Shop
                        orderDto.ShopId, // Buyer Shop
                        "ShopToShopSell",
                        orderDto.TotalQty,
                        orderDto.TotalAmount,
                        orderDto.Products,
                        "Success"
                    );
                }
                else
                {
                    // Call service function with proper parameters
                    orderId = await _orders.CreateOrderAsync(
                        orderDto.CompanyId,
                        orderDto.ShopId,
                        "CompanyToShop",
                        orderDto.TotalQty,
                        orderDto.TotalAmount,
                        orderDto.Products,
                        "Success"
                    );
                }

                return Ok(new { message = "Order placed successfully!", orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error placing order: " + ex.Message);
            }
        }

    }
}
