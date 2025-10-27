using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Auth;
using ERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenService _jwt;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwt)
        {
            _userManager = userManager; _roleManager = roleManager; _jwt = jwt;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            if (dto.Email == "admin@erp.local")
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");
            }
            var roles = await _userManager.GetRolesAsync(user);
            return new AuthResponseDto(_jwt.Generate(user.Id, user.Email!, Array.Empty<string>()), user.Email!);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null || !(await _userManager.CheckPasswordAsync(user, dto.Password)))
                throw new InvalidOperationException("Geçersiz kimlik bilgileri.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwt.Generate(user.Id, user.Email!, roles);
            return new AuthResponseDto(token, user.Email!);
        }

        public async Task<IEnumerable<AuthUserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<AuthUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new AuthUserDto(user.Id, user.Email, roles));
            }

            return result;
        }


    }
}
