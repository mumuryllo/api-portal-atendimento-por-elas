namespace portal_por_elas.Domain.Entities
{
    public class Evento
    {
        protected Evento() { }

        public Evento(string nome, DateOnly dataEvento, TimeOnly horaInicio, TimeOnly horaFim)
        {
            Id = Guid.NewGuid();
            DefinirNome(nome);
            DefinirHorario(dataEvento, horaInicio, horaFim);
            Ativo = true;
            CriadoEm = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public DateOnly DataEvento { get; private set; }
        public TimeOnly HoraInicio { get; private set; }
        public TimeOnly HoraFim { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime CriadoEm { get; private set; }

        private readonly List<FilaAtendimento> _atendimentos = new();
        public IReadOnlyCollection<FilaAtendimento> Atendimentos => _atendimentos.AsReadOnly();

        public void DefinirNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do evento é obrigatório.");

            Nome = nome.Trim();
        }

        public void DefinirHorario(DateOnly dataEvento, TimeOnly horaInicio, TimeOnly horaFim)
        {
            if (horaFim <= horaInicio)
                throw new ArgumentException("A hora final deve ser maior que a hora de início.");

            DataEvento = dataEvento;
            HoraInicio = horaInicio;
            HoraFim = horaFim;
        }

        public void Desativar() => Ativo = false;
    }
}