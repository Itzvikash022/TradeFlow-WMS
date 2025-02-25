using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using iText.Layout;
using System;
using WMS_Application.DTO;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using iText.IO.Font;
using MailKit.Search;
using System.Transactions;
namespace WMS_Application.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly dbMain _context;
        private readonly IEmailSenderRepository _emailSender;
        public OrdersRepository(dbMain context, IEmailSenderRepository emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<List<TblOrder>> GetAllOrders(int loggedInShopId)
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

                                where o.SellerId == loggedInShopId || o.BuyerId == loggedInShopId

                                select new TblOrder
                                {
                                    OrderId = o.OrderId,
                                    OrderDate = o.OrderDate,
                                    OrderType = o.OrderType,
                                    SellerId = o.SellerId,
                                    BuyerId = o.BuyerId,
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
                                    CanEditStatus = o.SellerId == loggedInShopId,
                                    TotalQty = o.TotalQty,
                                    PaymentStatus = o.PaymentStatus
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
        public async Task<int> CreateOrderAsync(int sellerId, int buyerId, string orderType, int totalQty, decimal totalAmount, List<ProductDto> products, string status, string paymentStatus)
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
                TotalQty = totalQty,
                PaymentStatus = paymentStatus
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


            return orderId;
        }

        // Generalized stock update function
        public async Task UpdateStockAsync(string orderType, List<ProductDto> products, int sellerId, int buyerId)
        {
            //Check qty
            foreach (var product in products)
            {
                if (orderType == "CompanyToShop")
                {
                    // Reduce stock from tblProducts (Company stock)
                    var companyProduct = await _context.TblProducts
                        .FirstOrDefaultAsync(p => p.ProductId == product.ProductID);

                    if (companyProduct != null && companyProduct.ProductQty < product.qty)
                    {
                        throw new Exception($"Insufficient stock for selected product");
                    }
                }
                else if (orderType == "ShopToCustomer")
                {
                    // Reduce stock from the seller's shop (tblStock)
                    var shopStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == sellerId);

                    if (shopStock != null && shopStock.Quantity < product.qty)
                    {
                        throw new Exception($"Insufficient stock for product ID {product.ProductID} in shop.");
                    }
                }
                else if (orderType == "ShopToShopBuy" || orderType == "ShopToShopSell")
                {
                    // Reduce stock from the seller's shop (Shop 1)
                    var sellerStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == sellerId);

                    if (sellerStock != null && sellerStock.Quantity < product.qty)
                    {
                        throw new Exception($"Insufficient stock for product ID {product.ProductID} in seller shop.");
                    }
                }
            }

            //Update qty
            foreach (var product in products)
            {
                if (orderType == "CompanyToShop")
                {
                    // Reduce stock from tblProducts (Company stock)
                    var companyProduct = await _context.TblProducts
                        .FirstOrDefaultAsync(p => p.ProductId == product.ProductID);

                        companyProduct.ProductQty -= product.qty;

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

                        shopStock.Quantity -= product.qty;
                }
                else if (orderType == "ShopToShopBuy" || orderType == "ShopToShopSell")
                {
                    // Reduce stock from the seller's shop (Shop 1)
                    var sellerStock = await _context.TblStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == sellerId);

                        sellerStock.Quantity -= product.qty;

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


        //Saving Transaction Info
        public async Task<object> AddTransactionInfo(TblTransaction transaction)
        {
            try
            {
                var order = await _context.TblOrders
                    .FirstOrDefaultAsync(u => u.OrderId == transaction.OrderId);

                if (order == null)
                    return new { success = false, message = $"Order Not Found" };


                // ✅ Pre-check stock BEFORE inserting the transaction
                var products = await _context.TblOrderDetails
                  .Where(od => od.OrderId == transaction.OrderId)
                  .Select(od => new ProductDto
                  {
                      ProductID = (int)od.ProductId,
                      PricePerUnit = od.PricePerUnit,
                      qty = od.Quantity
                  })
                  .ToListAsync();

                foreach (var product in products)
                {
                    if (order.OrderType == "CompanyToShop")
                    {
                        var companyProduct = await _context.TblProducts
                            .FirstOrDefaultAsync(p => p.ProductId == product.ProductID);

                        if (companyProduct == null || companyProduct.ProductQty < product.qty)
                            //return new { success = false, message = $"Insufficient stock for Product ID {product.ProductID}" };
                            throw new Exception($"Insufficient stock for Product ID {product.ProductID}");
                    }
                    else if (order.OrderType == "ShopToCustomer")
                    {
                        var shopStock = await _context.TblStocks
                            .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == order.SellerId);

                        if (shopStock == null || shopStock.Quantity < product.qty)
                            //return new { success = false, message = $"Insufficient stock for Product ID {product.ProductID} in shop." };
                            throw new Exception($"Insufficient stock for Product ID {product.ProductID} in shop.");
                    }
                    else if (order.OrderType == "ShopToShopBuy" || order.OrderType == "ShopToShopSell")
                    {
                        var sellerStock = await _context.TblStocks
                            .FirstOrDefaultAsync(s => s.ProductId == product.ProductID && s.ShopId == order.SellerId);

                        if (sellerStock == null || sellerStock.Quantity < product.qty)
                            //return new { success = false, message = $"Insufficient stock for Product ID {product.ProductID} in seller shop." };
                            throw new Exception($"Insufficient stock for Product ID {product.ProductID} in seller shop.");
                    }
                }


                // ✅ Generate receipt & send email AFTER everything is successfully saved
                if (transaction.OrderId != 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Receipts");
                    string uniqueFileName = $"Receipt_{transaction.OrderId}_{Guid.NewGuid()}.pdf";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var receiptBytes = await GenerateReceiptAsync(transaction);
                    if (receiptBytes != null)
                    {
                        await File.WriteAllBytesAsync(filePath, receiptBytes);
                        transaction.ReceiptPath = "\\Receipts\\" + uniqueFileName;

                        string buyerEmail;
                        if (order.OrderType == "ShopToCustomer")
                        {
                            buyerEmail = await _context.TblCustomers
                                .Where(u => u.CustomerId == order.BuyerId)
                                .Select(u => u.Email)
                                .FirstOrDefaultAsync();
                        }
                        else
                        {
                            int buyerId = await _context.TblShops
                                .Where(u => u.ShopId == order.BuyerId)
                                .Select(u => u.AdminId)
                                .FirstOrDefaultAsync();

                            buyerEmail = await _context.TblUsers
                                .Where(u => u.UserId == buyerId)
                                .Select(u => u.Email)
                                .FirstOrDefaultAsync();
                        }

                        string fileName = $"Receipt_{transaction.OrderId}.pdf";
                        await _emailSender.SendEmailAsync(
                            toEmail: buyerEmail,
                            subject: "Your Order Receipt",
                            body: "<p>Thank you for your order! Find your receipt attached.</p>",
                            attachmentBytes: receiptBytes,
                            attachmentFilename: fileName
                        );
                    }
                }

                // ✅ Stock is available, proceed with transaction insertion
                await _context.TblTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                // ✅ Update stock AFTER transaction is successfully saved
                await UpdateStockAsync(order.OrderType, products, (int)order.SellerId, (int)order.BuyerId);

                // ✅ Now update order status & commit changes
                order.PaymentStatus = "Paid";

                if (order.OrderType != "ShopToShopBuy")
                {
                    order.OrderStatus = "Success";
                }

                _context.TblOrders.Update(order);
                await _context.SaveChangesAsync();


                // ✅ Generate receipt & send email AFTER everything is successfully saved
                if (transaction.OrderId != 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Receipts");
                    string uniqueFileName = $"Receipt_{transaction.OrderId}_{Guid.NewGuid()}.pdf";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var receiptBytes = await GenerateReceiptAsync(transaction);
                    if (receiptBytes != null)
                    {
                        await File.WriteAllBytesAsync(filePath, receiptBytes);
                        transaction.ReceiptPath = "\\Receipts\\" + uniqueFileName;

                        string buyerEmail;
                        if (order.OrderType == "ShopToCustomer")
                        {
                            buyerEmail = await _context.TblCustomers
                                .Where(u => u.CustomerId == order.BuyerId)
                                .Select(u => u.Email)
                                .FirstOrDefaultAsync();
                        }
                        else
                        {
                            int buyerId = await _context.TblShops
                                .Where(u => u.ShopId == order.BuyerId)
                                .Select(u => u.AdminId)
                                .FirstOrDefaultAsync();

                            buyerEmail = await _context.TblUsers
                                .Where(u => u.UserId == buyerId)
                                .Select(u => u.Email)
                                .FirstOrDefaultAsync();
                        }

                        string fileName = $"Receipt_{transaction.OrderId}.pdf";
                        await _emailSender.SendEmailAsync(
                            toEmail: buyerEmail,
                            subject: "Your Order Receipt",
                            body: "<p>Thank you for your order! Find your receipt attached.</p>",
                            attachmentBytes: receiptBytes,
                            attachmentFilename: fileName
                        );
                    }
                }

                return new { success = true, message = "Transaction added successfully" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); // Actually throw the exception
            }

        }


        public async Task<byte[]> GenerateReceiptAsync(TblTransaction transaction)
        {
            var order = _context.TblOrders.FirstOrDefault(o => o.OrderId == transaction.OrderId);
            //var orderDetails = _context.TblOrderDetails.Where(od => od.OrderId == orderId).ToList();

            var orderDetails = _context.TblOrderDetails
                .Where(od => od.OrderId == transaction.OrderId)
                .Select(od => new
                {
                    od.ProductId,
                    ProductName = _context.TblProducts
                        .Where(p => p.ProductId == od.ProductId)
                        .Select(p => p.ProductName)
                        .FirstOrDefault(),
                    od.Quantity,
                    od.PricePerUnit,
                    od.Amount
                })
                .ToList();


            if (order == null || orderDetails.Count == 0)
                return null;

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Receipts");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, $"Receipt_{transaction.OrderId}.pdf");

            try
            {
                // **Ensure Proper File Closing**
                using (PdfWriter writer = new PdfWriter(filePath))
                using (PdfDocument pdf = new PdfDocument(writer))
                using (Document document = new Document(pdf))
                {
                    //PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);

                    document.Add(new Paragraph("Order Receipt").SetFont(boldFont).SetFontSize(20));

                    document.Add(new Paragraph($"Order ID: {order.OrderId}"));
                    document.Add(new Paragraph($"Date: {order.OrderDate}"));
                    document.Add(new Paragraph($"Buyer : {transaction.BuyerName}"));
                    document.Add(new Paragraph($"Seller : {transaction.SellerName}"));
                    document.Add(new Paragraph($"Total Amount: Rs.{order.TotalAmount}"));
                    document.Add(new Paragraph($"Payment Reference ID: {transaction.ReferenceNo}"));
                    document.Add(new Paragraph($"Mode of Payment: {transaction.TransactionType}"));
                    document.Add(new Paragraph(" "));

                    Table table = new Table(4);
                    table.AddCell("Product Name");
                    table.AddCell("Quantity");
                    table.AddCell("Price/Unit");
                    table.AddCell("Total");

                    foreach (var item in orderDetails)
                    {
                        table.AddCell(item.ProductName ?? "N/A");
                        table.AddCell(item.Quantity.ToString());
                        table.AddCell($"Rs. {item.PricePerUnit}");
                        table.AddCell($"Rs. {item.Amount}");
                    }

                    document.Add(table);
                    document.Add(new Paragraph("\nThank you for your purchase!"));
                }

                // **Wait Before Reading File to Avoid Locks**
                await Task.Delay(500);

                // **Ensure File is Available Before Reading**
                if (!File.Exists(filePath))
                    throw new Exception("PDF file was not created successfully.");

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating receipt: {ex.Message}");
                return null;
            }
        }


        public OrderDetailsDTO GetOrderDetails(int orderId)
        {
            // Fetch Order Details
            var order = _context.TblOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return null;

            // Fetch Transaction Details

            //var transaction = _context.TblTransactions.FirstOrDefault(t => t.OrderId == orderId);
            var transaction = _context.TblTransactions
            .Where(t => t.OrderId == orderId)
            .Select(t => new
            {
                ReferenceNo = t.ReferenceNo ?? "N/A",
                TransactionType = t.TransactionType ?? "N/A",
                Remarks = t.Remarks ?? "N/A"
            })
            .FirstOrDefault();


            // Fetch Seller & Buyer Details
            string sellerName = "", sellerAddress = "", sellerEmail = "", buyerName = "", buyerContact = "", buyerEmail = "";

            if (order.OrderType == "CompanyToShop")
            {
                var company = _context.TblCompanies.FirstOrDefault(c => c.CompanyId == order.SellerId);
                if (company != null)
                {
                    sellerName = company.CompanyName;
                    sellerAddress = company.Address;
                    sellerEmail = company.Email;
                }
            }
            else
            {
                var seller = _context.TblShops.FirstOrDefault(u => u.ShopId == order.SellerId);
                var user = _context.TblUsers.FirstOrDefault(_context => _context.UserId == seller.AdminId);
                if (seller != null)
                {
                    sellerName = seller.ShopName;
                    sellerAddress = seller.Address;
                    sellerEmail = user.Email;
                }
            }

            if (order.OrderType == "ShopToCustomer")
            {
                var customer = _context.TblCustomers.FirstOrDefault(c => c.CustomerId == order.BuyerId);
                if (customer != null)
                {
                    buyerName = customer.CustomerName;
                    buyerContact = customer.Contact;
                    buyerEmail = customer.Email;
                }
            }
            else
            {
                var buyer = _context.TblShops.FirstOrDefault(u => u.ShopId == order.BuyerId);
                var user = _context.TblUsers.FirstOrDefault(_context => _context.UserId == buyer.AdminId);
                if (buyer != null)
                {
                    buyerName = buyer.ShopName;
                    buyerContact = user.PhoneNumber;
                    buyerEmail = user.Email;
                }
            }

            // Fetch Products in the Order
            var orderProducts = _context.TblOrderDetails
                .Where(od => od.OrderId == orderId)
                .Select(od => new OrderProductDTO
                {
                    ProductId = (int)od.ProductId,
                    Quantity = od.Quantity,
                    PricePerUnit = od.PricePerUnit,
                    TotalAmount = od.Amount,
                    ProductName = _context.TblProducts.Where(p => p.ProductId == od.ProductId).Select(p => p.ProductName).FirstOrDefault() ?? "N/a",
                    ProductImage = _context.TblProducts.Where(p => p.ProductId == od.ProductId).Select(p => p.ProductImagePath).FirstOrDefault() ?? "N/A"
                })
                .ToList();

            // Construct the DTO
            return new OrderDetailsDTO
            {
                // Transaction Data
                ReferenceNumber = transaction?.ReferenceNo,
                PaymentType = transaction?.TransactionType,
                Remarks = transaction?.Remarks,
                // Order Data
                OrderId = order.OrderId,
                OrderDate = (DateTime)order.OrderDate,
                OrderType = order.OrderType,
                TotalAmount = order.TotalAmount,
                TotalQuantity = order.TotalQty,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                BuyerId = (int) order.BuyerId,

                // Seller & Buyer Data
                SellerName = sellerName,
                SellerAddress = sellerAddress,
                SellerEmail = sellerEmail,
                BuyerName = buyerName,
                BuyerContact = buyerContact,
                BuyerEmail = buyerEmail,

                // Product List
                Products = orderProducts
            };

        }
    }
}
