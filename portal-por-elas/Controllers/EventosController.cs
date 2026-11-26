using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portal_por_elas.Contracts.Request.Evento;
using portal_por_elas.Contracts.Response.Evento;
using portal_por_elas.Contracts.Response.Servico;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventosController : ControllerBase
    {
        private readonly IGestaoFilaService _gestaoFilaService;

        public EventosController(IGestaoFilaService gestaoFilaService)
        {
            _gestaoFilaService = gestaoFilaService;
        }

        /// <summary>
        /// Painel ADM: cria um novo evento (somente ADMIN).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        public async Task<ActionResult<EventoResponseDto>> Criar([FromBody] CriarEventoRequestDto dto)
        {
            var criado = await _gestaoFilaService.CriarEventoAsync(dto);
            return CreatedAtAction(nameof(ObterAtivos), new { id = criado.Id }, criado);
        }

        /// <summary>
        /// Lista eventos ativos (acessível para ADMIN e Mulheres).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{AuthorizationConstants.Admin},{AuthorizationConstants.Mulheres}")]
        public async Task<ActionResult<IEnumerable<EventoResponseDto>>> ObterAtivos()
        {
            return Ok(await _gestaoFilaService.ObterEventosAtivosAsync());
        }

        /// <summary>
        /// Lista serviços disponíveis (acessível para ADMIN e Mulheres).
        /// </summary>
        [HttpGet("servicos")]
        [Authorize(Roles = $"{AuthorizationConstants.Admin},{AuthorizationConstants.Mulheres}")]
        public async Task<ActionResult<IEnumerable<ServicoResponseDto>>> ObterServicos()
        {
            return Ok(await _gestaoFilaService.ObterServicosAsync());
        }
    }
}