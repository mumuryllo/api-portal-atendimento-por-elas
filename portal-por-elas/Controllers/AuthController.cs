using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Extensions;
using portal_por_elas.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace portal_por_elas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Autentica o usuário e retorna AccessToken (JWT) + RefreshToken.
        /// Limitado a 5 tentativas por minuto por IP.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtension.PoliticaAuthRestrito)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Renova o AccessToken expirado usando o RefreshToken válido.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingExtension.PoliticaAuthRestrito)]
        public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var response = await _authService.RefreshTokenAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Logout: revoga o RefreshToken do usuário no banco de dados.
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _authService.RevokeAsync(userId);
            return NoContent();
        }
    }
}