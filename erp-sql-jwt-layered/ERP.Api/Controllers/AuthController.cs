using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IAuthService auth, UserManager<ApplicationUser> userManager)
    {
        _auth = auth;
        _userManager = userManager;
    }

    /// <summary>
    /// Yeni kullanýcý kaydý oluþturur.
    /// </summary>
    /// <param name="dto">Register DTO</param>
    /// <returns>AuthResponseDto</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var res = await _auth.RegisterAsync(dto);
        return Ok(res);
    }

    /// <summary>
    /// Kullanýcý login olur ve JWT token alýr.
    /// </summary>
    /// <param name="dto">Login DTO</param>
    /// <returns>AuthResponseDto içinde JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var res = await _auth.LoginAsync(dto);
        if (res == null || string.IsNullOrEmpty(res.Token))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(res);
    }

    /// <summary>
    /// Sadece Admin kullanýcýlarý görebilir.
    /// </summary>
    /// <returns>Liste halinde Admin kullanýcýlar</returns>
    [HttpGet("admins")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<object>>> GetAdminUsers()
    {
        var users = _userManager.Users.ToList();
        var admins = new List<object>();

        foreach (var user in users)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                admins.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed
                });
            }
        }

        return Ok(admins);
    }

    [HttpGet("all-users")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<IEnumerable<AuthUserDto>>> GetAllUsers()
    {
        var users = await _auth.GetAllUsersAsync();
        return Ok(users);
    }
}
