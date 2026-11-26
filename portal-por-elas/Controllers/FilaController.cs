using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.Fila;
using portal_por_elas.Contracts.Request.Status;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Contracts.Response.Evento;
using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Contracts.Response.Painel;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Events;
using portal_por_elas.Interfaces.Services;
using System.Text.Json;

namespace portal_por_elas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FilaController : ControllerBase
    {
        private readonly IGestaoFilaService _gestaoFilaService;

        public FilaController(IGestaoFilaService gestaoFilaService)
        {
            _gestaoFilaService = gestaoFilaService;
        }

        /// <summary>
        /// Painel das Mulheres: lista a fila ativa e a chamada atual ("CHEGOU SUA VEZ!").
        /// Requer permissão Mulheres ou ADMIN.
        /// </summary>
        [HttpGet("painel-mulheres")]
        [Authorize(Roles = $"{AuthorizationConstants.Mulheres},{AuthorizationConstants.Admin}")]
        public async Task<ActionResult<PainelPublicoResponseDto>> ObterPainelMulheres([FromQuery] Guid? eventoId = null)
        {
            return Ok(await _gestaoFilaService.ObterPainelPublicoAsync(eventoId));
        }

        /// <summary>
        /// Painel das Mulheres (Tempo Real via SSE): escuta o event C# AoChamarMulher e envia push instantâneo.
        /// Requer permissão Mulheres ou ADMIN.
        /// </summary>
        [HttpGet("stream-chamadas")]
        [Authorize(Roles = $"{AuthorizationConstants.Mulheres},{AuthorizationConstants.Admin}")]
        public async Task StreamChamadas(
            [FromServices] IFilaNotificacaoService filaNotificacao,
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            async Task Handler(ChamadaAtendimentoEventArgs args)
            {
                var json = JsonSerializer.Serialize(args);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            filaNotificacao.AoChamarMulher += Handler;

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Cliente encerrou a conexão normalmente
            }
            finally
            {
                filaNotificacao.AoChamarMulher -= Handler;
            }
        }

        /// <summary>
        /// Painel ADM: lista todos os eventos com suas filas e status (somente ADMIN).
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        public async Task<ActionResult<IEnumerable<EventoComFilaResponseDto>>> ObterFilaAdmin()
        {
            return Ok(await _gestaoFilaService.ObterFilaAdminPorEventosAsync());
        }

        /// <summary>
        /// Painel ADM: cadastra uma mulher na fila de um evento com um ou mais serviços (somente ADMIN).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        public async Task<ActionResult<ItemFilaAtendimentoResponseDto>> CadastrarMulher([FromBody] CadastrarMulherFilaRequestDto dto)
        {
            var resultado = await _gestaoFilaService.CadastrarMulherNaFilaAsync(dto);
            return Created(string.Empty, resultado);
        }

        /// <summary>
        /// Painel ADM: chama automaticamente a próxima mulher da Queue (FIFO) (somente ADMIN).
        /// </summary>
        [HttpPost("evento/{eventoId:guid}/chamar-proxima")]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        public async Task<ActionResult<ItemFilaAtendimentoResponseDto>> ChamarProxima(Guid eventoId)
        {
            var chamada = await _gestaoFilaService.ChamarProximaDaFilaAsync(eventoId);
            return Ok(chamada);
        }

        /// <summary>
        /// Painel ADM: altera o status de um atendimento específico (somente ADMIN).
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        public async Task<ActionResult<ItemFilaAtendimentoResponseDto>> AtualizarStatus(
            Guid id,
            [FromBody] AtualizarStatusAtendimentoRequestDto dto)
        {
            var atualizado = await _gestaoFilaService.AtualizarStatusAsync(id, dto.Status);
            return Ok(atualizado);
        }

        /// <summary>
        /// Remove do banco de dados os atendimentos com status Finalizado há mais de 1 hora.
        /// </summary>
        [HttpDelete("finalizados/expirados")]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LimparFinalizadosExpirados()
        {
            var removidos = await _gestaoFilaService.LimparAtendimentosFinalizadosExpiradosAsync();
            return Ok(new
            {
                removidos,
                mensagem = $"{removidos} atendimento(s) finalizado(s) há mais de 1 hora foram removido(s)."
            });
        }
    }
}