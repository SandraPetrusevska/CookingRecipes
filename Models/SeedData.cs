using Microsoft.AspNetCore.Identity;
using CookingRecipes.Areas.Identity.Data;

namespace CookingRecipes.Models
{
    public class SeedData
    {
        public static async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<CookingRecipesUser>>();
            IdentityResult roleResult;
            //Add Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin")); }
            var roleCheck2 = await RoleManager.RoleExistsAsync("User");
            if (!roleCheck2) { roleResult = await RoleManager.CreateAsync(new IdentityRole("User")); }
            CookingRecipesUser user = await UserManager.FindByEmailAsync("admin@cookingrecipes.com");
            if (user == null)
            {
                var User = new CookingRecipesUser();
                User.Email = "admin@cookingrecipes.com";
                User.UserName = "admin@cookingrecipes.com";
                string userPWD = "Admin123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Admin"); }
            }
        }
        public static void Initialize(IServiceProvider serviceProvider)
        {
            {
                CreateUserRoles(serviceProvider).Wait();
            }


        }
    }
}
