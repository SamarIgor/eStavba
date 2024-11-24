using Microsoft.AspNetCore.Identity;

namespace eStavba.Services
{
    public class RoleService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RoleService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task AssignRole(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null && !await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
