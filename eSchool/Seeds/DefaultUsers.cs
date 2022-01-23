using eSchool.Constants;
using eSchool.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Seeds
{
    public static class DefaultUsers
    {

        public async static Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var defaultUser = new ApplicationUser
            {
                UserName = "admin"
            };
            var user = await userManager.FindByNameAsync(defaultUser.UserName);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "admin2@D");
                await userManager.AddToRolesAsync(defaultUser,
                    new List<string> {
                        Roles.Admin.ToString(),
                        Roles.Student.ToString(),
                        Roles.Teacher.ToString(),
                        Roles.Parent.ToString() });
            }
        }



    }
}
