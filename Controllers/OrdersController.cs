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
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Data;
using System.Diagnostics;

namespace WMS_Application.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly IOrdersRepository _orders;
        private readonly dbMain _context;
        private readonly IProductRepository _product;
        private readonly ICustomerRepository _customer;
        private readonly IEmailSenderRepository _emailSender;
        private readonly IPermisionHelperRepository _permission;
        private readonly IExportServiceRepository _export;
        private readonly IActivityRepository _activity;

        public OrdersController(ISidebarRepository sidebar, IOrdersRepository orders, dbMain context, IProductRepository product, ICustomerRepository customer, IEmailSenderRepository emailSender, IPermisionHelperRepository permission, IExportServiceRepository export, IActivityRepository activity) : base(sidebar)
        {
            _orders = orders;
            _context = context;
            _product = product;
            _customer = customer;
            _emailSender = emailSender;
            _permission = permission;
            _export = export;
            _activity = activity;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string PermissionType = _permission.HasAccess(action, roleId);
            ViewBag.Permissiontype = PermissionType;
            return PermissionType;
        }


        //caliing this function to fetch all orders, converted this into a fn to call it in export and index both actions
        public async Task<List<TblOrder>> GetAllOrders()
        {
            List<TblOrder> orders = new List<TblOrder>();
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");

            if (roleId != 5)
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

                foreach (var order in orders)
                {
                    if (order.SellerId == ShopData.ShopId)
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
                int companyId = (int)HttpContext.Session.GetInt32("CompanyId");
                orders = await _orders.GetAllCompanyOrders(companyId);
                foreach (var order in orders)
                {
                    if (order.SellerId == companyId)
                    {
                        order.OrderType = "Sale";
                    }
                }
            }
            return orders;
        }

        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Orders");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                List<TblOrder> orders = await GetAllOrders();
                return View(orders);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


        public async Task<IActionResult> ExportOrders()
        {
            List<TblOrder> orders = await GetAllOrders();


            var dataTable = new DataTable("Companies");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Buyer Name"),
                new DataColumn("Seller Name"),
                new DataColumn("Order Status"),
                new DataColumn("Order Date"),
                new DataColumn("Total Amount"),
                new DataColumn("Order Quantity"),
                new DataColumn("Payment Status"),
            });

            foreach (var order in orders)
            {
                dataTable.Rows.Add(order.BuyerName, order.SellerName, order.OrderStatus, order.OrderDate, order.TotalAmount, order.TotalQty, order.PaymentStatus);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "Orders");

            if (fileBytes != null)
            {
                //rechecking the session not reusing the upper ones cuz, it was a mess changing bit might crash something so im not touching it

                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                string name = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();

                string type = "Export Orders List";
                string desc = $"{name} exported Orders List";

                _activity.AddNewActivity(userId, roleId, type, desc);
            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
        }


        [HttpGet]
        //Order Details View
        public IActionResult OrderDetails(int id)
        {
            string permissionType = GetUserPermission("Orders");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View(_orders.GetOrderDetails(id));
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }


        //Order Create View
        public IActionResult CreateOrder()
        {
            string permissionType = GetUserPermission("Orders");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                List<TblProductCategory> prodCat = _context.TblProductCategories.Where(x => x.IsActive == true).ToList();
                ViewBag.ProductCategory = prodCat;
                int adminId = (int)HttpContext.Session.GetInt32("UserId");
                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                if (roleId > 2 && roleId != 5)
                {
                    adminId = (int)_context.TblUsers.Where(x => x.UserId == adminId).Select(adminId => adminId.AdminRef).FirstOrDefault();
                }
                var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
                HttpContext.Session.SetInt32("UserShopId", ShopData.ShopId);
                int currentAdminId = (int)HttpContext.Session.GetInt32("UserId");
                var shops = _orders.GetShopDetails(currentAdminId);
                ViewBag.ShopData = shops;
                var company = _context.TblCompanies.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                ViewBag.CompanyData = company;
                return View();
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
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
            string permissionType = GetUserPermission("Orders");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int shopId = (int)HttpContext.Session.GetInt32("ShopId");

                //TblOrder OrderDetails = new TblOrder();
                //var AllOrders = await _orders.GetAllOrders(shopId);
                var OrderDetails = _orders.GetOrderDetails(orderId);
                if (OrderDetails.PaymentStatus == "Paid")
                {
                    return RedirectToAction("Index");
                }
                return View(OrderDetails);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        //Fetching All Products based on their Companies
        public IActionResult GetFilteredProducts(string? productName, int? category, int? company)
        {
            var result = _orders.GetProductsC2S(productName, category, company);
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

        //Fetching All Products of other shops
        public IActionResult GetFilteredProductsShop(string? productName, int? category, int? shop)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
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
            if (result == null)
            {
                return Ok(new { text = "No products available." });
            }

            return Ok(new
            {
                msg,
                result,
                customerId = CustomerId
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
            if (roleId > 2 && roleId != 5)
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



            int prodCount = orderDto.Products.Count();
            string type = "", desc = "", customerName = "";
            string userName = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.Username).FirstOrDefault();
            if (roleId > 2 && roleId != 5)
            {
                adminId = _context.TblUsers.Where(x => x.UserId == adminId).Select(y => y.AdminRef).FirstOrDefault();
            }
            var ShopData = _context.TblShops.FirstOrDefault(x => x.AdminId == adminId);
            try
            {
                int orderId = 0;
                if (orderDto.CustomerId > 0)
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
                    type = "Sold";
                    customerName = _context.TblCustomers.Where(x => x.CustomerId == orderDto.CustomerId).Select(y => y.CustomerName).FirstOrDefault();
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

                    type = "Bought";
                    customerName = _context.TblShops.Where(x => x.ShopId == orderDto.SellerShopId).Select(y => y.ShopName).FirstOrDefault();

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

                    type = "Sold";
                    customerName = _context.TblShops.Where(x => x.ShopId == orderDto.ShopId).Select(y => y.ShopName).FirstOrDefault();

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
                    
                    type = "Bought";
                    customerName = _context.TblCompanies.Where(x => x.CompanyId == orderDto.CompanyId).Select(y => y.CompanyName).FirstOrDefault();

                    string email = _context.TblCompanies.Where(x => x.CompanyId == orderDto.CompanyId).Select(y => y.Email).FirstOrDefault();
                    string buyerName = _context.TblShops.Where(x => x.ShopId == orderDto.ShopId).Select(y => y.ShopName).FirstOrDefault();
                    string body = $"New order request received from {buyerName}, checkout your order page for more details";
                    _emailSender.SendEmailAsync(email, "New Order Request Received", body);

                }
                TempData["order-toast"] = "Order Place Successfully";
                TempData["order-toastType"] = "success";

                string verb = type == "Sold" ? "To" : "From";
                desc = $"{userName} {type} {prodCount} Products {verb} {customerName} with total quantity being {orderDto.TotalQty} worth of {orderDto.TotalAmount}";
                _activity.AddNewActivity(adminId, roleId, $"Products {type}", desc);

                return Ok(new { success = true, message = "Order placed successfully!", orderId });
                //return RedirectToAction($"OrderCheckout/{orderId}");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
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

                    int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                    int id = 0;
                    string name = "";
                    if(roleId == 5)
                    {
                        id = (int)HttpContext.Session.GetInt32("CompanyId");
                        name = _context.TblCompanies.Where(x => x.CompanyId == id).Select(y => y.CompanyName).FirstOrDefault();
                    }
                    else
                    {
                        id = (int)HttpContext.Session.GetInt32("UserId");
                        name = _context.TblUsers.Where(x => x.UserId == id).Select(y => y.Username).FirstOrDefault();
                    }
                    string buyerName = _context.TblShops.Where(x => x.ShopId == order.BuyerId).Select(y => y.ShopName).FirstOrDefault();

                    string type = "Order Request Status Updated";
                    string desc = $"{name} accepted {buyerName}'s order request";
                    _activity.AddNewActivity(id, roleId, type, desc);

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
                TempData["transaction-toast"] = "Transaction added successfully!";
                TempData["transaction-toastType"] = "success";

                int id = (int)HttpContext.Session.GetInt32("UserId");
                int shopId = (int)HttpContext.Session.GetInt32("ShopId");
                string userName = _context.TblUsers.Where(x => x.UserId == id).Select(y => y.Username).FirstOrDefault();
                string shopName = _context.TblShops.Where(x => x.ShopId == shopId).Select(y => y.ShopName).FirstOrDefault();

                int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
                var order = _context.TblOrders.FirstOrDefault(x => x.OrderId == transaction.OrderId);
                string type = "Manual Add Payment";
                string desc = $"{userName} manually added payment details for {shopName} for OrderID : {transaction.OrderId}";
                if(order.BuyerId == shopId)
                {
                    type = "Add Payment";
                    desc = $"{shopName} added it's payment details for OrderID : {transaction.OrderId}";
                }

                return Ok(new {success = true, message = "Transaction added successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}
