using portal_por_elas.Contracts.Response;
using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Domain.Events;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Services
{
    public class FilaNotificacaoService : IFilaNotificacaoService
    {
        private readonly Dictionary<Guid, Queue<ItemFilaAtendimentoResponseDto>> _filasPorEvento = new();
        private readonly object _lock = new();

        public event Func<ChamadaAtendimentoEventArgs, Task>? AoChamarMulher;

        public void SincronizarFila(Guid eventoId, IEnumerable<ItemFilaAtendimentoResponseDto> itensAguardando)
        {
            lock (_lock)
            {
                _filasPorEvento[eventoId] = new Queue<ItemFilaAtendimentoResponseDto>(
                    itensAguardando.OrderBy(i => i.CriadoEm)
                );
            }
        }

        public void Enfileirar(ItemFilaAtendimentoResponseDto item)
        {
            lock (_lock)
            {
                if (!_filasPorEvento.TryGetValue(item.EventoId, out var fila))
                {
                    fila = new Queue<ItemFilaAtendimentoResponseDto>();
                    _filasPorEvento[item.EventoId] = fila;
                }

                if (fila.All(x => x.Id != item.Id))
                    fila.Enqueue(item);
            }
        }

        public ItemFilaAtendimentoResponseDto? EspiarProxima(Guid eventoId)
        {
            lock (_lock)
            {
                return _filasPorEvento.TryGetValue(eventoId, out var fila) && fila.Count > 0
                    ? fila.Peek()
                    : null;
            }
        }

        public ItemFilaAtendimentoResponseDto? DesenfileirarProxima(Guid eventoId)
        {
            lock (_lock)
            {
                return _filasPorEvento.TryGetValue(eventoId, out var fila) && fila.Count > 0
                    ? fila.Dequeue()
                    : null;
            }
        }

        public void RemoverDaFila(Guid eventoId, Guid atendimentoId)
        {
            lock (_lock)
            {
                if (!_filasPorEvento.TryGetValue(eventoId, out var fila))
                    return;

                var restantes = fila.Where(x => x.Id != atendimentoId).ToList();
                _filasPorEvento[eventoId] = new Queue<ItemFilaAtendimentoResponseDto>(restantes);
            }
        }

        public IReadOnlyCollection<ItemFilaAtendimentoResponseDto> ObterFilaAtual(Guid eventoId)
        {
            lock (_lock)
            {
                return _filasPorEvento.TryGetValue(eventoId, out var fila)
                    ? fila.ToList().AsReadOnly()
                    : Array.Empty<ItemFilaAtendimentoResponseDto>();
            }
        }

        public async Task NotificarChamadaAsync(ChamadaAtendimentoEventArgs args)
        {
            if (AoChamarMulher is not null)
            {
                var handlers = AoChamarMulher.GetInvocationList()
                    .Cast<Func<ChamadaAtendimentoEventArgs, Task>>();

                foreach (var handler in handlers)
                {
                    await handler(args);
                }
            }
        }
    }
}