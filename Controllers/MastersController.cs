using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class MastersController : BaseController
    {
        private readonly dbMain _context;
        private readonly IPermisionHelperRepository _permission;

        public MastersController(dbMain context, ISidebarRepository sidebar, IPermisionHelperRepository permission) : base(sidebar)
        {
            _context = context;
            _permission = permission;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        public IActionResult ProductCategory()
        {
            string permissionType = GetUserPermission("Product Category");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                var productCategory = _context.TblProductCategories.Where(x => x.IsDeleted == false).ToList();
                return View(productCategory);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public IActionResult DeleteProductCategory(int id)
        {
            var productCategory = _context.TblProductCategories.Where(x => x.ProdCatId == id).FirstOrDefault();

            productCategory.IsDeleted = true;
            productCategory.IsActive = false;
            _context.TblProductCategories.Update(productCategory);
            _context.SaveChanges();
            return RedirectToAction("ProductCategory");
        }

        [HttpPost]
        public IActionResult SaveProductCategories([FromBody] TblProductCategory prodCat)
        {
            if (prodCat.ProdCatId == 0) // New entry
            {
                _context.TblProductCategories.Add(prodCat);
            }
            else // Update existing
            {
                prodCat.IsDeleted = false;
                _context.TblProductCategories.Update(prodCat);
            }

            _context.SaveChanges();
            return Ok();
        }
        
        public IActionResult Roles()
        {
            string permissionType = GetUserPermission("Roles Master");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                var roles = _context.TblRoles.Where(x => x.RoleId > 2 && x.RoleId != 5 && x.IsDeleted == false).ToList();
                return View(roles);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public IActionResult SaveRoles([FromBody] TblRole role)
        {
            if(role.RoleId == 0)
            {
                _context.TblRoles.Add(role);
            }
            else
            {
                _context.TblRoles.Update(role);
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult DeleteRoles(int id)
        {
            var roles = _context.TblRoles.Where(x => x.RoleId == id).FirstOrDefault();
            
            roles.IsDeleted = true;
            roles.IsActive = false;
            _context.Update(roles);
            _context.SaveChanges();
            return RedirectToAction("Roles");
        }
        
        public IActionResult Access()
        {
            string permissionType = GetUserPermission("Access Master");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                var tabs = _context.TblTabs.Where(x => x.IsActive == true).ToList();
                var roles = _context.TblRoles.Where(x => x.IsActive == true).ToList();

                ViewBag.Tabs = tabs;
                ViewBag.Roles = roles;
                return View(new List<TblPermission>());
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public JsonResult GetRolePermissions(int roleId)
        {
            var permissions = _context.TblPermissions
                .Where(p => p.RoleId == roleId)
                .Select(p => new {
                    p.PermissionId,
                    p.TabId,
                    TabName = _context.TblTabs.Where(t => t.TabId == p.TabId).Select(t => t.TabName).FirstOrDefault(),
                    p.PermissionType,
                    p.IsActive
                }).ToList();
            return Json(permissions);
        }

        [HttpPost]
        public IActionResult SavePermission([FromBody] TblPermission permission)
        {
            string msg = "";
            bool isSuccess = true;
            bool alreadyExists = _context.TblPermissions.Any(x => x.TabId == permission.TabId && x.RoleId == permission.RoleId);
            if (alreadyExists && permission.PermissionId == 0)
            {
                isSuccess = false;
                msg = "This Permission already exists you can edit it from the list";
            }
            else if(permission.PermissionId == 0)
            {
                _context.TblPermissions.Add(permission);
                msg = "Permission Added Successfully";
            }
            else
            {
                _context.TblPermissions.Update(permission);
                msg = "Permission Updated Successfully";
            }
            _context.SaveChanges();
            return Ok(new { success = isSuccess, message = msg });
        }

        [HttpGet]
        public IActionResult DeletePermission(int id)
        {
            var permission = _context.TblPermissions.Where(x => x.PermissionId == id).FirstOrDefault();
            _context.TblPermissions.Remove(permission);
            _context.SaveChanges();
            return RedirectToAction("Access");
        }

    }
}
