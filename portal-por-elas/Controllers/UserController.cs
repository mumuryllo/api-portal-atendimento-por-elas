using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("Usuários")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Cadastra um novo usuário no sistema.
        /// </summary>
        /// <param name="dto">Dados para criação do usuário.</param>
        /// <returns>Dados do usuário criado.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto)
        {
            var createdUser = await _userService.CreateAsync(dto);
            return Created(string.Empty, createdUser);
        }

        /// <summary>
        /// Atualiza a senha de um usuário a partir do e-mail cadastrado.
        /// </summary>
        /// <param name="dto">E-mail e nova senha.</param>
        [HttpPatch("password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDto dto)
        {
            await _userService.UpdatePasswordAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Remove um usuário pelo identificador único.
        /// </summary>
        /// <param name="id">ID (Guid) do usuário.</param>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}