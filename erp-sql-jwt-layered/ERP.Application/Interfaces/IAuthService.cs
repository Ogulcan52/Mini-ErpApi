using ERP.Application.DTOs;

namespace ERP.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<IEnumerable<AuthUserDto>> GetAllUsersAsync();

    }
}
