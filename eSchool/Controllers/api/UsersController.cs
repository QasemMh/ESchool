using eSchool.Constants;
using eSchool.Data;
using eSchool.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public UsersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var student = user.StudentId;
            var parent = user.ParentId;
            var teacher = user.TeacherId;

            try
            {
                if (student != null)
                {
                    _context.Students.Remove(await _context.Students.FindAsync(student));
                }
                else if (parent != null)
                {
                    _context.Parents.Remove(await _context.Parents.FindAsync(parent));
                }
                else if (teacher != null)
                {
                    _context.Teachers.Remove(await _context.Teachers.FindAsync(teacher));
                }
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
            }
            catch (Exception)
            {
                throw;
            }

            return Ok();
        }




    }
}
