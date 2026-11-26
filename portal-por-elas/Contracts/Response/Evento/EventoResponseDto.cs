namespace portal_por_elas.Contracts.Response.Evento
{
    public class EventoResponseDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateOnly DataEvento { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFim { get; set; }
    }
}
