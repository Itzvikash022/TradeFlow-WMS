using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class AdminsController : BaseController
    {
        private readonly dbMain _context;
        private readonly IAdminsRepository _admins;
        public AdminsController(dbMain context, ISidebarRepository sidebar, IAdminsRepository admins) : base(sidebar)
        {
            _context = context;
            _admins = admins;
        }
        [Route("Admins")]
        public async Task<IActionResult> Index()
        {
            return View(await _admins.GetAllAdminsData());
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            TblUser admin= _context.TblUsers.FirstOrDefault(x => x.UserId == id);
            TblAdminInfo adminInfo = _context.TblAdminInfos.FirstOrDefault(x => x.AdminId == id);
            TblShop shopInfo = _context.TblShops.FirstOrDefault(x => x.AdminId == id);

            ViewBag.UserDetails = admin;
            ViewBag.AdminInfo = adminInfo;
            ViewBag.ShopInfo = shopInfo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int AdminId, bool Status)
        {   
            var result = await _admins.UpdateStatus(AdminId, Status);
            return Json(result);
        }

        //[HttpGet]
        //public async Task<IActionResult> SaveAdmin(int? id)
        //{
        //    TblUser model = new TblUser();

        //    if (id > 0)
        //    {
        //        model = await _context.TblUsers.FirstOrDefaultAsync(x => x.UserId == id);
        //    }
        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> SaveAdmin(TblUser user)
        //{
        //    if (user.UserId > 0)
        //    {
        //        _context.TblUsers.Update(user);
        //    }
        //    else
        //    {
        //        _context.TblUsers.Add(user);
        //    }
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}
