using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.Evento;
using portal_por_elas.Contracts.Request.Fila;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Contracts.Response.Evento;
using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Contracts.Response.Painel;
using portal_por_elas.Contracts.Response.Servico;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Domain.Enums;
using portal_por_elas.Domain.Events;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Services
{
    public class GestaoFilaService : IGestaoFilaService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFilaNotificacaoService _filaNotificacao;

        public GestaoFilaService(IUnitOfWork uow, IFilaNotificacaoService filaNotificacao)
        {
            _uow = uow;
            _filaNotificacao = filaNotificacao;
        }

        public async Task<EventoResponseDto> CriarEventoAsync(CriarEventoRequestDto dto)
        {
            var novoEvento = new Evento(dto.Nome, dto.DataEvento, dto.HoraInicio, dto.HoraFim);
            await _uow.Eventos.AddAsync(novoEvento);
            await _uow.CommitAsync();

            return new EventoResponseDto
            {
                Id = novoEvento.Id,
                Nome = novoEvento.Nome,
                DataEvento = novoEvento.DataEvento,
                HoraInicio = novoEvento.HoraInicio,
                HoraFim = novoEvento.HoraFim
            };
        }

        public async Task<IEnumerable<EventoResponseDto>> ObterEventosAtivosAsync()
        {
            var eventos = await _uow.Eventos.ObterEventosAtivosAsync();
            return eventos.Select(e => new EventoResponseDto
            {
                Id = e.Id,
                Nome = e.Nome,
                DataEvento = e.DataEvento,
                HoraInicio = e.HoraInicio,
                HoraFim = e.HoraFim
            });
        }

        public async Task<IEnumerable<ServicoResponseDto>> ObterServicosAsync()
        {
            var servicos = await _uow.Servicos.ObterTodosAtivosAsync();
            return servicos.Select(s => new ServicoResponseDto
            {
                Id = s.Id,
                Nome = s.Nome
            });
        }

        public async Task<ItemFilaAtendimentoResponseDto> CadastrarMulherNaFilaAsync(CadastrarMulherFilaRequestDto dto)
        {
            var evento = await _uow.Eventos.GetByIdAsync(dto.EventoId)
                ?? throw new NotFoundException("Evento não encontrado.");

            var servicosValidos = (await _uow.Servicos.ObterPorIdsAsync(dto.ServicosIds)).ToList();
            if (servicosValidos.Count != dto.ServicosIds.Distinct().Count())
                throw new NotFoundException("Um ou mais serviços selecionados são inválidos.");

            var atendimento = new FilaAtendimento(evento.Id, dto.NomeMulher, servicosValidos.Select(s => s.Id));
            await _uow.FilaAtendimentos.AddAsync(atendimento);
            await _uow.CommitAsync();

            var itemDto = new ItemFilaAtendimentoResponseDto
            {
                Id = atendimento.Id,
                EventoId = atendimento.EventoId,
                NomeMulher = atendimento.NomeMulher,
                Status = atendimento.Status,
                DescricaoStatus = ObterDescricaoStatus(atendimento.Status),
                Servicos = servicosValidos.Select(s => s.Nome).ToList(),
                CriadoEm = atendimento.CriadoEm,
                ChamadoEm = atendimento.ChamadoEm
            };

            _filaNotificacao.Enfileirar(itemDto);

            return itemDto;
        }

        public async Task<ItemFilaAtendimentoResponseDto> ChamarProximaDaFilaAsync(Guid eventoId)
        {
            if (!_filaNotificacao.ObterFilaAtual(eventoId).Any())
            {
                var pendentesBanco = await _uow.FilaAtendimentos.ObterPainelPublicoPorEventoAsync(eventoId);
                _filaNotificacao.SincronizarFila(
                    eventoId,
                    pendentesBanco
                        .Where(x => x.Status == StatusAtendimento.Aguardando)
                        .Select(MapearAtendimento)
                );
            }

            var proxima = _filaNotificacao.DesenfileirarProxima(eventoId)
                ?? throw new NotFoundException("Não há mulheres aguardando na fila deste evento.");

            return await AtualizarStatusAsync(proxima.Id, StatusAtendimento.EmAtendimento);
        }

        public async Task<ItemFilaAtendimentoResponseDto> AtualizarStatusAsync(Guid atendimentoId, StatusAtendimento novoStatus)
        {
            var atendimento = await _uow.FilaAtendimentos.ObterPorIdComServicosAsync(atendimentoId)
                ?? throw new NotFoundException("Atendimento não encontrado na fila.");

            switch (novoStatus)
            {
                case StatusAtendimento.Aguardando:
                    atendimento.ColocarEmEspera();
                    break;
                case StatusAtendimento.EmAtendimento:
                    atendimento.ChamarParaAtendimento();
                    break;
                case StatusAtendimento.Finalizado:
                    atendimento.FinalizarAtendimento();
                    break;
                default:
                    throw new ArgumentException("Status de atendimento inválido.");
            }

            await _uow.FilaAtendimentos.UpdateAsync(atendimento);
            await _uow.CommitAsync();

            var dto = MapearAtendimento(atendimento);

            if (novoStatus == StatusAtendimento.Aguardando)
                _filaNotificacao.Enfileirar(dto);
            else
                _filaNotificacao.RemoverDaFila(atendimento.EventoId, atendimento.Id);

            if (novoStatus == StatusAtendimento.EmAtendimento)
            {
                await _filaNotificacao.NotificarChamadaAsync(new ChamadaAtendimentoEventArgs
                {
                    AtendimentoId = dto.Id,
                    EventoId = dto.EventoId,
                    NomeMulher = dto.NomeMulher,
                    Servicos = dto.Servicos,
                    ChamadoEm = dto.ChamadoEm ?? DateTime.UtcNow
                });
            }

            return dto;
        }

        public async Task<IEnumerable<EventoComFilaResponseDto>> ObterFilaAdminPorEventosAsync()
        {
            var eventos = await _uow.Eventos.ObterEventosComFilaAsync();

            return eventos.Select(e => new EventoComFilaResponseDto
            {
                EventoId = e.Id,
                TituloEvento = $"{e.Nome} - {e.DataEvento:dd/MM/yyyy}",
                Fila = e.Atendimentos
                    .OrderBy(a => a.CriadoEm)
                    .Select(MapearAtendimento)
                    .ToList()
            });
        }

        public async Task<PainelPublicoResponseDto> ObterPainelPublicoAsync(Guid? eventoId = null)
        {
            var lista = eventoId.HasValue
                ? await _uow.FilaAtendimentos.ObterPainelPublicoPorEventoAsync(eventoId.Value)
                : await _uow.FilaAtendimentos.ObterTodoPainelPublicoAsync();

            var itens = lista.Select(MapearAtendimento).ToList();

            var chamadaAtual = itens
                .Where(i => i.Status == StatusAtendimento.EmAtendimento)
                .OrderByDescending(i => i.ChamadoEm)
                .FirstOrDefault();

            return new PainelPublicoResponseDto
            {
                ChamadaAtual = chamadaAtual,
                Fila = itens
            };
        }

        private static ItemFilaAtendimentoResponseDto MapearAtendimento(FilaAtendimento f) => new()
        {
            Id = f.Id,
            EventoId = f.EventoId,
            NomeMulher = f.NomeMulher,
            Status = f.Status,
            DescricaoStatus = ObterDescricaoStatus(f.Status),
            Servicos = f.Servicos.Select(s => s.Servico.Nome).ToList(),
            CriadoEm = f.CriadoEm,
            ChamadoEm = f.ChamadoEm
        };

        private static string ObterDescricaoStatus(StatusAtendimento status) => status switch
        {
            StatusAtendimento.Aguardando => "Aguardando atendimento",
            StatusAtendimento.EmAtendimento => "Atendida",
            StatusAtendimento.Finalizado => "Finalizado",
            _ => status.ToString()
        };
    }
}