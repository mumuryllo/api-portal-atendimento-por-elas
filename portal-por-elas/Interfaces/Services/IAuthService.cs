using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;

namespace portal_por_elas.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task RevokeAsync(Guid userId);
    }
}