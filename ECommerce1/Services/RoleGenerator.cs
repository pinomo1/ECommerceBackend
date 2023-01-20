using ECommerce1.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce1.Services
{
    public class RoleGenerator
    {
        public RoleGeneratorOptions Options { get; }
        public RoleGenerator(IOptions<RoleGeneratorOptions> options)
        {
            Options = options.Value;
        }

        public async Task<bool> AddDefaultRoles(RoleManager<IdentityRole> roleManager)
        {
            IdentityRole taskResult;
            foreach (string role in Options.Roles)
            {
                taskResult = await roleManager.FindByNameAsync(role);
                if (taskResult == null)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
