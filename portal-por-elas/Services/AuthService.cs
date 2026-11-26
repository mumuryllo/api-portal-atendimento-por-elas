
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace portal_por_elas.Services
{
    namespace portal_por_elas.Services
    {
        public class AuthService : IAuthService
        {
            private readonly IUnitOfWork _uow;
            private readonly ITokenService _tokenService;

            public AuthService(IUnitOfWork uow, ITokenService tokenService)
            {
                _uow = uow;
                _tokenService = tokenService;
            }

            public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
            {
                var user = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

                if (user is null || !user.Active)
                    throw new UnauthorizedException("Usuário ou senha inválidos.");

                if (user.IsTemporaryPasswordExpired())
                    throw new UnauthorizedException("Sua senha temporária expirou. Solicite uma nova senha.");

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    throw new UnauthorizedException("Usuário ou senha inválidos.");

                var (accessToken, expiraEm) = _tokenService.GenerateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.DefinirRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
                await _uow.Users.UpdateAsync(user);
                await _uow.CommitAsync();

                return new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiraEm = expiraEm,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };
            }

            public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    throw new UnauthorizedException("Token inválido.");

                var user = await _uow.Users.GetByIdAsync(userId);

                if (user is null || !user.Active || !user.RefreshTokenValido(dto.RefreshToken))
                    throw new UnauthorizedException("Refresh token inválido ou expirado. Faça login novamente.");

                // Rotação de tokens: gera um novo par e invalida o anterior
                var (novoAccessToken, expiraEm) = _tokenService.GenerateToken(user);
                var novoRefreshToken = _tokenService.GenerateRefreshToken();

                user.DefinirRefreshToken(novoRefreshToken, DateTime.UtcNow.AddDays(7));
                await _uow.Users.UpdateAsync(user);
                await _uow.CommitAsync();

                return new LoginResponseDto
                {
                    AccessToken = novoAccessToken,
                    RefreshToken = novoRefreshToken,
                    AccessTokenExpiraEm = expiraEm,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };
            }

            public async Task RevokeAsync(Guid userId)
            {
                var user = await _uow.Users.GetByIdAsync(userId)
                    ?? throw new NotFoundException("Usuário não encontrado.");

                user.RevogarRefreshToken();
                await _uow.Users.UpdateAsync(user);
                await _uow.CommitAsync();
            }
        }
    }
}
