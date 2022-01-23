using eSchool.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var role = await _roleManager.Roles.ToListAsync();
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Add(RoleFormViewModel role)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", await _roleManager.Roles.ToListAsync());
            }
            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                ModelState.AddModelError("Name", "Role Already Exists");
                return View("Index", await _roleManager.Roles.ToListAsync());
            }
            await _roleManager.CreateAsync(new IdentityRole
            {
                Name = role.Name.Trim()
            });
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Remove(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            RoleViewModel model = new RoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            return View(model);
        }
        [ActionName("Remove")]
        [HttpPost]
        public async Task<IActionResult> ConfirmRemove(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

    }
}
