using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Domain.Events;

namespace portal_por_elas.Interfaces.Services
{
    public interface IFilaNotificacaoService
    {
        event Func<ChamadaAtendimentoEventArgs, Task>? AoChamarMulher;

        void SincronizarFila(Guid eventoId, IEnumerable<ItemFilaAtendimentoResponseDto> itensAguardando);
        void Enfileirar(ItemFilaAtendimentoResponseDto item);
        ItemFilaAtendimentoResponseDto? EspiarProxima(Guid eventoId);
        ItemFilaAtendimentoResponseDto? DesenfileirarProxima(Guid eventoId);
        void RemoverDaFila(Guid eventoId, Guid atendimentoId);
        IReadOnlyCollection<ItemFilaAtendimentoResponseDto> ObterFilaAtual(Guid eventoId);
        Task NotificarChamadaAsync(ChamadaAtendimentoEventArgs args);
    }
}