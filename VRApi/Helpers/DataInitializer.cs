using Microsoft.AspNetCore.Identity;
using VRApi.Data;
using VRApi.Models;

namespace VRApi.Helpers
{
    public static class DataInitializer
    {
        public static async Task SeedData(ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        public static async Task SeedUsers(UserManager<User> userManager)
        {
            var requiredUser = await userManager.FindByNameAsync("admin");
            if (requiredUser is null)
            {
                var user = new User()
                {
                    UserName = "admin",
                };
                await userManager.CreateAsync(user, "Admin_1234");
                await userManager.AddToRoleAsync(user, "admin");
            }
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("user"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("user"));
            }
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("admin"));
            }
        }
    }
}
