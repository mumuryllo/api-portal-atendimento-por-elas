using portal_por_elas.Contracts.Response.Fila;

namespace portal_por_elas.Contracts.Response.Painel
{
    public class PainelPublicoResponseDto
    {
        public ItemFilaAtendimentoResponseDto? ChamadaAtual { get; set; }
        public List<ItemFilaAtendimentoResponseDto> Fila { get; set; } = new();
    }
}
