using portal_por_elas.Contracts.Response.Fila;

namespace portal_por_elas.Contracts.Response.Evento
{
    public class EventoComFilaResponseDto
    {
        public Guid EventoId { get; set; }
        public string TituloEvento { get; set; } = string.Empty;
        public List<ItemFilaAtendimentoResponseDto> Fila { get; set; } = new();
    }
}
