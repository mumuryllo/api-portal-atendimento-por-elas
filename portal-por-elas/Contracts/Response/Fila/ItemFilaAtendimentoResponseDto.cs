using portal_por_elas.Domain.Enums;

namespace portal_por_elas.Contracts.Response.Fila
{
    public class ItemFilaAtendimentoResponseDto
    {
        public Guid Id { get; set; }
        public Guid EventoId { get; set; }
        public string NomeMulher { get; set; } = string.Empty;
        public StatusAtendimento Status { get; set; }
        public string DescricaoStatus { get; set; } = string.Empty;
        public List<string> Servicos { get; set; } = new();
        public DateTime CriadoEm { get; set; }
        public DateTime? ChamadoEm { get; set; }
    }
}
