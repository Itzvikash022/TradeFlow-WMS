using Microsoft.AspNetCore.Mvc;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;

namespace WMS_Application.Controllers
{
    public class MastersController : BaseController
    {
        private readonly dbMain _context;
        private readonly IPermisionHelperRepository _permission;
        private readonly IActivityRepository _activity;

        public MastersController(dbMain context, ISidebarRepository sidebar, IPermisionHelperRepository permission, IActivityRepository activity) : base(sidebar)
        {
            _context = context;
            _permission = permission;
            _activity = activity;
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

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string type = "Product Category Delete";
            string desc = $"{userName} deleted a product category : {productCategory.ProductCategory}";
            _activity.AddNewActivity(userId, roleId, type, desc);
            return RedirectToAction("ProductCategory");
        }

        [HttpPost]
        public IActionResult SaveProductCategories([FromBody] TblProductCategory prodCat)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string type = "", desc = "";
            if (prodCat.ProdCatId == 0) // New entry
            {
                _context.TblProductCategories.Add(prodCat);
                type = "Product Category Add";
                desc = $"{userName} added a new product category : {prodCat.ProductCategory}";
            }
            else // Update existing
            {
                prodCat.IsDeleted = false;
                _context.TblProductCategories.Update(prodCat);
                type = "Product Category Update";
                desc = $"{userName} updated the product category : {prodCat.ProductCategory}";
            }

            _context.SaveChanges();
            _activity.AddNewActivity(roleId, roleId, type, desc);
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
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string type = "", desc = "";
            if (role.RoleId == 0)
            {
                _context.TblRoles.Add(role);
                type = "User Role Add";
                desc = $"{userName} added a new User Role : {role.RoleName}";
            }
            else
            {
                _context.TblRoles.Update(role);
                type = "User Role Update";
                desc = $"{userName} updated a User Role : {role.RoleName}";
            }
            _activity.AddNewActivity(userId, roleId, type, desc);

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

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string type = "User Role Delete";
            string desc = $"{userName} deleted a user role : {roles.RoleName}";
            _activity.AddNewActivity(userId, roleId, type, desc);

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

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string roleName = _context.TblRoles.Where(x => x.RoleId == permission.RoleId).Select(y => y.RoleName).FirstOrDefault();
            string type = "", desc = "";

            if (alreadyExists && permission.PermissionId == 0)
            {
                isSuccess = false;
                msg = "This Permission already exists you can edit it from the list";
            }
            else if(permission.PermissionId == 0)
            {
                _context.TblPermissions.Add(permission);
                msg = "Permission Added Successfully";
                type = "New Permission Add";
                desc = $"{userName} added a new access permission to role : {roleName}";
            }
            else
            {
                _context.TblPermissions.Update(permission);
                msg = "Permission Updated Successfully";
                type = "Role Permission Update";
                desc = $"{userName} updated access permission of role : {roleName}";
            }

            _activity.AddNewActivity(userId, roleId, type, desc);
            _context.SaveChanges();
            return Ok(new { success = isSuccess, message = msg });
        }

        [HttpGet]
        public IActionResult DeletePermission(int id)
        {
            var permission = _context.TblPermissions.Where(x => x.PermissionId == id).FirstOrDefault();
            _context.TblPermissions.Remove(permission);
            _context.SaveChanges();

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            string userName = _context.TblUsers.Where(x => x.UserId == userId).Select(y => y.Username).FirstOrDefault();
            string roleName = _context.TblRoles.Where(x => x.RoleId == permission.RoleId).Select(y => y.RoleName).FirstOrDefault();

            string type = "Role Permission Delete";
            string desc = $"{userName} deleted a access permisson for role : {roleName}";
            _activity.AddNewActivity(userId, roleId, type, desc);
            return RedirectToAction("Access");
        }

    }
}
