using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portal_por_elas.Contracts.Response.Hateoas;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("HATEOAS & Navegação")]
    public class NavigationController : ControllerBase
    {
        private readonly IHateoasService _hateoasService;

        public NavigationController(IHateoasService hateoasService)
        {
            _hateoasService = hateoasService;
        }

        /// <summary>
        /// Retorna os links HATEOAS dos endpoints disponíveis de acordo com a role do usuário e o ambiente.
        /// </summary>
        /// <param name="simulateRole">
        /// (Apenas Development) Permite simular a visualização de links para uma role específica.
        /// </param>
        [HttpGet("links")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HateoasResponseDto), StatusCodes.Status200OK)]
        public ActionResult<HateoasResponseDto> GetLinks([FromQuery] string? simulateRole = null)
        {
            var response = _hateoasService.DiscoverLinks(User, simulateRole);
            return Ok(response);
        }
    }
}