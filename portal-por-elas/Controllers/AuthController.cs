namespace portal_por_elas.Controllers
{
    using global::portal_por_elas.Contracts.Request;
    using global::portal_por_elas.Contracts.Response;
    using global::portal_por_elas.Interfaces.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
 
    namespace portal_por_elas.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Produces("application/json")]
        [Tags("Autenticação")]
        public class AuthController : ControllerBase
        {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            /// <summary>
            /// Realiza o login do usuário e retorna o token JWT.
            /// </summary>
            /// <param name="dto">Credenciais de acesso (e-mail e senha).</param>
            /// <returns>Token JWT e data de expiração.</returns>
            [HttpPost("login")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
        }
    }
}
