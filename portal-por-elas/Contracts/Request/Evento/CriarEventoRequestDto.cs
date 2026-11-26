namespace portal_por_elas.Contracts.Request.Evento
{
    public class CriarEventoRequestDto
    {
        public string Nome { get; set; } = string.Empty;
        public DateOnly DataEvento { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFim { get; set; }
    }
}