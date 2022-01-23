using eSchool.Constants;
using eSchool.Models;
using eSchool.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)

        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            var users = _userManager.Users
                .Select(user => new UserViewModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Roles = _userManager.GetRolesAsync(user).Result
                });

            int pageSize = 20;
            return View(await PaginatedList<UserViewModel>
                .CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> Add(string userId)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddUsersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(GetRolesList().Result, "Name", "Name");
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Email?.Trim()))
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    ViewBag.Roles = new SelectList(GetRolesList().Result, "Name", "Name");
                    return View(model);
                }

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError("Username", "Username is Already Exists");
                ViewBag.Roles = new SelectList(GetRolesList().Result, "Name", "Name");
                return View(model);
            }


            var userLogin = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.Phone
            };
            if (model.Role.Equals(Roles.Student.ToString()))
            {
                userLogin.Student = new Student
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NationalId = model.NationalId,
                    DateBirth = model.DateBrith.Date,
                    Gender = model.Gender
                };
            }
            else if (model.Role.Equals(Roles.Teacher.ToString()))
            {
                userLogin.Teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NationalId = model.NationalId,
                    DateBirth = model.DateBrith,
                    Gender = model.Gender
                };
            }
            else if (model.Role.Equals(Roles.Parent.ToString()))
            {
                userLogin.Parent = new Parent
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NationalId = model.NationalId,
                    DateBirth = model.DateBrith,
                    Gender = model.Gender
                };
            }


            var result = await _userManager.CreateAsync(userLogin, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Role", error.Description);
                }
                ViewBag.Roles = new SelectList(GetRolesList().Result, "Name", "Name");
                return View(model);
            }
            await _userManager.AddToRoleAsync(userLogin, model.Role);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            var ViewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(role => new RoleViewModel
                {
                    RoleName = role.Name,
                    RoleId = role.Id,
                    IsSelected = _userManager.IsInRoleAsync(user, role.Name).Result
                }).ToList()
            };

            return View(ViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }
            var usersRole = await _userManager.GetRolesAsync(user);
            foreach (var role in model.Roles)
            {
                if (usersRole.Any(r => r == role.RoleName) &&
                    !role.IsSelected)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                }

                if (!usersRole.Any(r => r == role.RoleName) &&
                     role.IsSelected)
                {
                    await _userManager.AddToRoleAsync(user, role.RoleName);
                }

            }
            return RedirectToAction(nameof(Index));
        }





        private async Task<List<IdentityRole>> GetRolesList()
        {
            return await _roleManager.Roles.ToListAsync();
        }

    }
}
