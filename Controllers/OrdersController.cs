using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using WMS_Application.Models;
using WMS_Application.DTO;
using WMS_Application.Repositories.Interfaces;
using MimeKit.Tnef;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Humanizer;

namespace WMS_Application.Controllers
{
    public class OrdersController : BaseController
    {
        private IOrdersRepository _orders;
        private dbMain _context;
        private IProductRepository _product;
        private ICustomerRepository _customer;
        private IEmailSenderRepository _emailSender;
        public OrdersController(ISidebarRepository sidebar, IOrdersRepository orders, dbMain context, IProductRepository product, ICustomerRepository customer, IEmailSenderRepository emailSender) : base(sidebar)
        {
            _orders = orders;
            _context = context;
            _product = product;
            _customer = customer;
            _emailSender = emailSender;
        }
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            
            List<TblOrder> orders = new List<TblOrder>();
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            
            if(roleId != 5)
            {
                int adminId = (int)HttpContext.Session.GetInt32("UserId");
                if (roleId > 2 && roleId != 5)
                {
                    int AdminRef = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
                }
                TblShop ShopData = new TblShop();
                if (roleId <= 2)
                {
                    ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
                }
                else
                {
                    int refId = _context.TblUsers.FirstOrDefault(x => x.UserId == adminId).AdminRef;
                    ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == refId);
                }
                if (ShopData != null)
                {
                    HttpContext.Session.SetInt32("ShopId", ShopData.ShopId);
                }
                orders = await _orders.GetAllOrders(ShopData.ShopId);

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
            }
            else
            {
                int companyId = (int) HttpContext.Session.GetInt32("CompanyId");
                orders = await _orders.GetAllCompanyOrders(companyId);
                foreach (var order in orders)
                {
                    if (order.SellerId == companyId)
                    {
                        order.OrderType = "Sale";
                    }
                }
            }
            return View(orders);
        }

        [HttpGet]
        //Order Details View
        public IActionResult OrderDetails(int id)
        {
            return View(_orders.GetOrderDetails(id));
        }


        //Order Create View
        public IActionResult CreateOrder()
        {
            List<TblProductCategory> prodCat = _context.TblProductCategories.Where(x => x.IsActive == true).ToList();
            ViewBag.ProductCategory = prodCat;
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int) HttpContext.Session.GetInt32("UserRoleId");
            if(roleId > 2 && roleId != 5)
            {
                adminId = (int)_context.TblUsers.Where(x => x.UserId == adminId).Select(adminId => adminId.AdminRef).FirstOrDefault();
            }
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            HttpContext.Session.SetInt32("UserShopId", ShopData.ShopId);
            int currentAdminId = (int)HttpContext.Session.GetInt32("UserId");
            var shops = _orders.GetShopDetails(currentAdminId);
            ViewBag.ShopData = shops;
            var company = _context.TblCompanies.Where(x=>x.IsActive == true && x.IsDeleted == false).ToList();
            ViewBag.CompanyData = company;
            return View();
        }

        public async Task<IActionResult> SendReminder(string buyerEmail)
        {
            _emailSender.SendEmailAsync(buyerEmail, "Payment Reminder", "Paise Baaki hai order ke bhar de");
            return Ok(new
            {
                message = "Reminder Email Sent Successfully"
            });
        }

        [HttpGet]
        public IActionResult DownloadReceipt(int orderId)
        {
            var receiptPath = _context.TblTransactions
                .Where(t => t.OrderId == orderId)
                .Select(t => t.ReceiptPath)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(receiptPath))
            {
                return NotFound(new { message = "Receipt not found for this order." });
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", receiptPath.TrimStart('\\'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "Receipt file missing from storage." });
            }

            // Return the file to the user
            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/pdf", Path.GetFileName(fullPath));
        }


        //Transaction Details + Reciept View Return
        [Route("orders/OrderCheckout/{orderId:int}")]
        [HttpGet]
        public async Task<IActionResult> OrderCheckout(int orderId)
        {
            int shopId = (int)HttpContext.Session.GetInt32("ShopId");

            //TblOrder OrderDetails = new TblOrder();
            //var AllOrders = await _orders.GetAllOrders(shopId);
            var OrderDetails = _orders.GetOrderDetails(orderId);
            if(OrderDetails.PaymentStatus == "Paid")
            {
                return RedirectToAction("Index");
            }
            return View(OrderDetails);
        }

        //Fetching All Products based on their Companies
        public IActionResult GetFilteredProducts(string? productName, int? category, int? company)
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

        //Fetching All Products of other shops
        public IActionResult GetFilteredProductsShop(string? productName, int? category, int? shop)
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


        //Fetching all my Shop Products
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

        //Saving or Checking Customer Details and Fetching all my shop products
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


        //Fetchsing all Shop Admin's details
        [HttpGet]
        public async Task<IActionResult> GetShopKeepersDetails(int selectedShopId)
        {
            var shopData = _orders.GetShopKeepersDetails(selectedShopId);
            return Json(shopData);
        }

        //Fetching all my shop Products
        [HttpGet]
        public async Task<IActionResult> GetMyShopProducts()
        {
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            if(roleId > 2 && roleId != 5)
            {
                adminId = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
            }
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            var result = _orders.GetAllProducts(0, ShopData.ShopId);
            if (result == null)
            {
                return Ok(new { text = "No products available." });
            }

            return Json(result);
        }


        //Create Order Action
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderDto)
        {
            if (orderDto == null || orderDto.Products == null || !orderDto.Products.Any())
            {
                return BadRequest("Invalid order data.");
            }
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            int adminId = (int)HttpContext.Session.GetInt32("UserId");
            if(roleId >2 && roleId != 5)
            {
                adminId = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
            }
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
                        "Pending",
                        "Pending"
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
                        "Pending",
                        "Pending"
                    );

                    string email = _context.TblUsers.Where(x => x.UserId == (_context.TblShops.Where(y => y.ShopId == orderDto.SellerShopId).Select(z => z.AdminId).FirstOrDefault())).Select(a => a.Email).FirstOrDefault();
                    string buyerName = _context.TblShops.Where(x => x.ShopId == orderDto.ShopId).Select(y => y.ShopName).FirstOrDefault();
                    string body = $"New order request received from {buyerName}, checkout your order page for more details";
                    _emailSender.SendEmailAsync(email, "New Order Request Received", body);
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
                        "Pending", 
                        "Pending"
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
                        "Pending",
                        "Pending"
                    );

                    string email = _context.TblCompanies.Where(x => x.CompanyId == orderDto.CompanyId).Select(y => y.Email).FirstOrDefault();
                    string buyerName = _context.TblShops.Where(x => x.ShopId == orderDto.ShopId).Select(y => y.ShopName).FirstOrDefault();
                    string body = $"New order request received from {buyerName}, checkout your order page for more details";
                    _emailSender.SendEmailAsync(email, "New Order Request Received", body);

                }

                return Ok(new { message = "Order placed successfully!", orderId });
                //return RedirectToAction($"OrderCheckout/{orderId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error placing order: " + ex.Message);
            }
        }

        //Updating qty of Products in tblStock
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
                    var productList = new List<ProductDto>(); // ✅ Store all products

                    // Loop through products in order and add to the list
                    foreach (var orderDetail in orderDetails)
                    {
                        productList.Add(new ProductDto
                        {
                            ProductID = (int)orderDetail.ProductId,
                            qty = orderDetail.Quantity,
                            PricePerUnit = orderDetail.PricePerUnit
                        });
                    }
                    _context.Update(order);
                    // Save the updated order status to the database
                    await _context.SaveChangesAsync();

                    // ✅ Now send the complete product list for stock update
                    await _orders.UpdateStockAsync(order.OrderId, productList);

                    return Json(new { success = true, message = "Order status updated successfully." });
                }
                return Json(new { success = true, message = "Order not successfull" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        //Add Transactions
        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] TblTransaction transaction)
            {
            try
            {
                await _orders.AddTransactionInfo(transaction);
                return Ok(new { message = "Transaction added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error adding transaction: " + ex.Message);
            }
        }
    }
}
