using portal_por_elas.Contracts.Request.Evento;
using portal_por_elas.Contracts.Request.Fila;
using portal_por_elas.Contracts.Response.Evento;
using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Contracts.Response.Painel;
using portal_por_elas.Contracts.Response.Servico;
using portal_por_elas.Domain.Enums;

namespace portal_por_elas.Interfaces.Services
{
    public interface IGestaoFilaService
    {
        Task<EventoResponseDto> CriarEventoAsync(CriarEventoRequestDto dto);
        Task<IEnumerable<EventoResponseDto>> ObterEventosAtivosAsync();
        Task<IEnumerable<ServicoResponseDto>> ObterServicosAsync();
        Task<ItemFilaAtendimentoResponseDto> CadastrarMulherNaFilaAsync(CadastrarMulherFilaRequestDto dto);
        Task<ItemFilaAtendimentoResponseDto> ChamarProximaDaFilaAsync(Guid eventoId);
        Task<ItemFilaAtendimentoResponseDto> AtualizarStatusAsync(Guid atendimentoId, StatusAtendimento novoStatus);
        Task<IEnumerable<EventoComFilaResponseDto>> ObterFilaAdminPorEventosAsync();
        Task<PainelPublicoResponseDto> ObterPainelPublicoAsync(Guid? eventoId = null);
    }
}