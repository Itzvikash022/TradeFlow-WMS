using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Data;
using System.Net.Http;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class ShopsController : BaseController
    {
        private readonly IShopRepository _shop;
        private readonly dbMain _context;
        private readonly IPermisionHelperRepository _permission;
        private readonly IExportServiceRepository _export;
        private readonly HttpClient _httpClient;


        public ShopsController(ISidebarRepository sidebar, IShopRepository shop, dbMain context, IPermisionHelperRepository permission, IExportServiceRepository export, IHttpClientFactory httpClientFactory) : base(sidebar)
        {
            _shop = shop;
            _context = context;
            _permission = permission;
            _export = export;
            _httpClient = httpClientFactory.CreateClient();

        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        [Route("Shops")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("Shops");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                return View(await _shop.GetAllShops());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet("/Shops/states")]
        public async Task<IActionResult> GetStates()
        {
            var response = await _httpClient.GetStringAsync("http://api.geonames.org/childrenJSON?geonameId=1269750&username=itzvikash");
            return Content(response, "application/json");
        }

        [HttpGet("/Shops/cities/{geonameId}")]
        public async Task<IActionResult> GetCities(int geonameId)
        {
            var url = $"http://api.geonames.org/childrenJSON?geonameId={geonameId}&username=itzvikash";
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
        }


        public async Task<IActionResult> ExportShopList()
        {
            List<TblShop> shopList = await _shop.GetAllShops();

            var dataTable = new DataTable("Shops");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Shop Name"),
                new DataColumn("State"),
                new DataColumn("City"),
                new DataColumn("Started On"),
                new DataColumn("Pincode"),
                new DataColumn("Address"),
                new DataColumn("Timings"),
                new DataColumn("AdminName")
            });

            foreach (var shop in shopList)
            {
                dataTable.Rows.Add(shop.ShopName, shop.State, shop.City, shop.CreatedAt, shop.Pincode, shop.Address, shop.StartTime + " to " + shop.ClosingTime, shop.ShopOwner);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "ShopList");
            if(fileBytes != null)
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                string username = _context.TblUsers.Where(x => x.UserId == id).Select(x => x.Username).FirstOrDefault();
                TblActivityLog activity = new TblActivityLog()
                {
                    UserId = id,
                    Role = (int)HttpContext.Session.GetInt32("UserRoleId"),
                    ActivityType = "Shop List Export",
                    Description = $"{username} exported Shop List"
                };
                _context.TblActivityLogs.Add(activity);
                _context.SaveChanges();

            }

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ShopList.xlsx");
        }

        [Route("MyShop")]
        public async Task<IActionResult> MyShop()
        {
            string permissionType = GetUserPermission("My Shop");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int UserId = (int)HttpContext.Session.GetInt32("UserId");
                TblShop shopData = await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == UserId);
                return View(shopData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet] //iska kuchh to krna padega bhai
        public async Task<IActionResult> SaveShop(int? id, bool? addNew)
        {
            string permissionType = GetUserPermission("Shops");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                TblShop shopData = new TblShop();

                if (addNew != null && addNew == true)
                {
                    //fetching admins whose shop data is not in tblshop
                    ViewBag.AdminList = await _context.TblUsers
                        .Where(x => x.RoleId == 2 && !_context.TblShops.Any(s => s.AdminId == x.UserId))
                        .ToListAsync();

                    ViewBag.IsAction = "SPAddNew";
                }

                else if (id == null || id == 0)
                {
                    int UserId = (int)HttpContext.Session.GetInt32("UserId");
                    ViewBag.IsAction = "UpdateSelf";
                    shopData = await _context.TblShops.FirstOrDefaultAsync(x => x.AdminId == UserId);
                }
                else if (id > 0)
                {
                    int adminId = await _context.TblShops.Where(x => x.ShopId == id).Select(x => x.AdminId).FirstOrDefaultAsync();
                    ViewBag.ShopOwner = await _context.TblUsers.Where(x => x.UserId == adminId).Select(x => x.Username).FirstOrDefaultAsync();
                    ViewBag.IsAction = "SPUpdate";
                    shopData = await _context.TblShops.FirstOrDefaultAsync(x => x.ShopId == id);
                }
                return View(shopData);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }
    }
}
