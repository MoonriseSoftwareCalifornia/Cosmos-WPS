using Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.IdentityManagement.Website.Controllers
{
    /// <summary>
    /// Role management controller
    /// </summary>
    //[ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrators")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        public RolesController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Role inventory
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index([Bind("ids")] string ids, string sortOrder = "asc", string currentSort = "Name", int pageNo = 0, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(ids))
            {
                ViewData["Ids"] = null;
            }
            else
            {
                ViewData["Ids"] = ids.Split(',');
            }

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var query = _roleManager.Roles.AsQueryable();

            ViewData["RowCount"] = await query.CountAsync();

            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Name":
                            query = query.OrderByDescending(o => o.Name);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Name":
                            query = query.OrderBy(o => o.Name);
                            break;
                    }
                }
            }

            var model = query.Skip(pageNo * pageSize).Take(pageSize);

            return View(await model.ToListAsync());
        }

        //public async Task<IActionResult> Create_Roles([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<IdentityRole> models)
        //{
        //    var results = new List<IdentityRole>();

        //    if (models != null && ModelState.IsValid)
        //    {
        //        foreach (var role in models)
        //        {
        //            role.Id = Guid.NewGuid().ToString();
        //            var result = await _roleManager.CreateAsync(role);

        //            if (result.Succeeded)
        //            {
        //                var identityRole = await _roleManager.FindByIdAsync(role.Id);

        //                await _roleManager.SetRoleNameAsync(identityRole, role.Name);
        //                await _roleManager.UpdateNormalizedRoleNameAsync(identityRole);
        //            }
        //            else
        //            {
        //                foreach (var error in result.Errors)
        //                {
        //                    ModelState.AddModelError("", $"Error code: {error.Code}. Message: {error.Description}");
        //                }
        //            }
        //        }
        //    }

        //    return Json(results.ToDataSourceResult(request, ModelState));
        //}


        ///// <summary>
        ///// Updates role names
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="users"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<ActionResult> Update_Roles([DataSourceRequest] DataSourceRequest request,
        //    [Bind(Prefix = "models")] IEnumerable<IdentityRole> roles)
        //{
        //    if (roles != null && ModelState.IsValid)
        //    {
        //        foreach (var role in roles)
        //        {
        //            var identityRole = await _roleManager.FindByIdAsync(role.Id);

        //            await _roleManager.SetRoleNameAsync(identityRole, role.Name);
        //            await _roleManager.UpdateNormalizedRoleNameAsync(identityRole);
        //        }
        //    }

        //    return Json(await roles.ToDataSourceResultAsync(request, ModelState));
        //}

        /// <summary>
        /// Deletes roles
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRoles(string[] roleIds)
        {
            var safeRoles = new string[] { "authors", "administrators", "authors", "editors", "reviewers" };

            var roles = await _roleManager.Roles
                .Where(w => safeRoles.Contains(w.Name.ToLower()) == false && roleIds.Contains(w.Id)).ToListAsync();

            if (roles != null && ModelState.IsValid)
            {
                foreach (var role in roles)
                {
                    var identityRole = await _roleManager.FindByIdAsync(role.Id);

                    await _roleManager.DeleteAsync(identityRole);
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gets users for a given email query
        /// </summary>
        /// <param name="startsWith"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetUsers(string startsWith)
        {
            var query = _userManager.Users.OrderBy(o => o.Email)
                .Select(
                  s => new
                  {
                      s.Id,
                      s.Email
                  }
                ).AsQueryable();

            if (!string.IsNullOrEmpty(startsWith))
            {
                query = query.Where(s => s.Email.ToLower().StartsWith(startsWith.ToLower()));
            }

            var users = await query.ToListAsync();

            return Json(users);
        }

        /// <summary>
        /// Page designed to add/remove users from a single role.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> UsersInRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var model = new UsersInRoleViewModel()
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            return View(model);
        }

        /// <summary>
        /// Saves changes to the user assignments in a role
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UsersInRole(UsersInRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var id in model.UserIds)
                {
                    var user = await _userManager.FindByIdAsync(id);
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }

                model.UserIds = null;

                return View(model);
            }

            // Not valid, return the selected users.
            model.Users = await _userManager.Users.Where(w => model.UserIds.Contains(w.Id))
                .Select(
                s => new SelectedUserViewModel()
                {
                    Id = s.Id,
                    Email = s.Email
                }
                ).ToListAsync();

            return View(model);
        }

        /// <summary>
        /// Removes users from a Role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUsers(string roleId, string[] userIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            foreach (var userId in userIds)
            {
                var identityUser = await _userManager.FindByIdAsync(userId);

                // Make sure there is at least one administrator remaining
                if (role.Name.Equals("User Administrators", StringComparison.InvariantCultureIgnoreCase))
                {
                    var administrators = await _userManager.GetUsersInRoleAsync("User Administrators");

                    if (administrators.Count() > 0)
                    {
                        await _userManager.RemoveFromRoleAsync(identityUser, role.Name);
                    }
                }
                else
                {
                    var result = await _userManager.RemoveFromRoleAsync(identityUser, role.Name);
                    var t = result;
                }
            }

            return RedirectToAction("Index");
        }

    }
}
