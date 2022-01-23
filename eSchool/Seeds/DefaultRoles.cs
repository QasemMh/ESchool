using eSchool.Constants;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Seeds
{
    public static class DefaultRoles
    {
        public async static Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
                await roleManager.CreateAsync(new IdentityRole(Roles.Teacher.ToString()));
                await roleManager.CreateAsync(new IdentityRole(Roles.Parent.ToString()));
                await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
            }
        }
    }
}
