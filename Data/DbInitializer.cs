using Microsoft.AspNetCore.Identity; 
using Microsoft.Extensions.DependencyInjection; 
using Hearty_Bites.Models; 
using System;
using System.Threading.Tasks;
public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();


        string[] roleNames = { "Admin", "Caterer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }


        var adminEmail = "admin@thecaterist.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new User { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(newAdmin, "Admin123!");
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}