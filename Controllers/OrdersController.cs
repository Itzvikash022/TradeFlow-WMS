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
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            HttpContext.Session.SetInt32("UserShopId", ShopData.ShopId);

            var orders = await _orders.GetAllOrders(ShopData.ShopId);

            foreach(var order in orders)
            {
                if(order.SellerId == ShopData.ShopId)
                {
                    order.OrderType = "Sale";
                }
                else
                {
                    order.OrderType = "Purchase";
                }
            }
            return View(orders);
        }

        public IActionResult CreateOrder()
        {
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            HttpContext.Session.SetInt32("UserShopId", ShopData.ShopId);
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
            int shopId = (int)HttpContext.Session.GetInt32("UserShopId");
            var result = _orders.GetAllProducts(0, shopId);
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
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequestDto request)
        {
           if (string.IsNullOrEmpty(request.NewStatus))
    {
        return Json(new { success = false, message = "Invalid status." });
    }


            try
            {
                // Fetch the order using the orderId
                var order = await _context.TblOrders.Include(o => o.TblOrderDetails)
                                                   .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Update the order status
                order.OrderStatus = request.NewStatus;

                // Only update stock if the status is "Success"
                if (request.NewStatus == "Success")
                {
                    // Fetch product details for stock updates
                    var orderDetails = order.TblOrderDetails;

                    // Loop through products in order and update stock
                    foreach (var orderDetail in orderDetails)
                    {
                        var product = new ProductDto
                        {
                            ProductID = (int) orderDetail.ProductId,
                            qty = orderDetail.Quantity,
                            PricePerUnit = orderDetail.PricePerUnit
                        };

                        // Reuse UpdateStockAsync to update seller and buyer stock
                        await _orders.UpdateStockAsync(order.OrderType, new List<ProductDto> { product }, (int)order.SellerId, (int)order.BuyerId);
                    }
                }

                // Save the updated order status to the database
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Order status updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



    }
}
