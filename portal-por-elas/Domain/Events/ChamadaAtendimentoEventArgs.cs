namespace portal_por_elas.Domain.Events
{
    public class ChamadaAtendimentoEventArgs : EventArgs
    {
        public Guid AtendimentoId { get; init; }
        public Guid EventoId { get; init; }
        public string NomeMulher { get; init; } = string.Empty;
        public List<string> Servicos { get; init; } = new();
        public DateTime ChamadoEm { get; init; }
    }
}