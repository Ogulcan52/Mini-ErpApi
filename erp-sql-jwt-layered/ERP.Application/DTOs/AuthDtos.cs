namespace ERP.Application.DTOs
{
    public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, string Email);
    public record AuthUserDto(string Id, string Email, IEnumerable<string> Roles);

}
