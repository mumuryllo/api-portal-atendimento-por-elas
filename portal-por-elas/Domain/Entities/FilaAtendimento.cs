using portal_por_elas.Domain.Enums;

namespace portal_por_elas.Domain.Entities
{
    public class FilaAtendimento
    {
        protected FilaAtendimento() { }

        public FilaAtendimento(Guid eventoId, string nomeMulher, IEnumerable<Guid> servicosIds)
        {
            Id = Guid.NewGuid();
            EventoId = eventoId;
            DefinirNomeMulher(nomeMulher);
            Status = StatusAtendimento.Aguardando;
            CriadoEm = DateTime.UtcNow;

            DefinirServicos(servicosIds);
        }

        public Guid Id { get; private set; }
        public Guid EventoId { get; private set; }
        public Evento Evento { get; private set; } = null!;
        public string NomeMulher { get; private set; } = string.Empty;
        public StatusAtendimento Status { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime? ChamadoEm { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }

        private readonly List<FilaAtendimentoServico> _servicos = new();
        public IReadOnlyCollection<FilaAtendimentoServico> Servicos => _servicos.AsReadOnly();

        public void DefinirNomeMulher(string nomeMulher)
        {
            if (string.IsNullOrWhiteSpace(nomeMulher))
                throw new ArgumentException("Nome da mulher é obrigatório.");

            NomeMulher = nomeMulher.Trim();
        }

        public void DefinirServicos(IEnumerable<Guid> servicosIds)
        {
            var idsUnicos = servicosIds?.Distinct().ToList() ?? new List<Guid>();
            if (!idsUnicos.Any())
                throw new ArgumentException("Selecione ao menos um serviço.");

            _servicos.Clear();
            foreach (var servicoId in idsUnicos)
            {
                _servicos.Add(new FilaAtendimentoServico(Id, servicoId));
            }
        }

        public void ColocarEmEspera()
        {
            Status = StatusAtendimento.Aguardando;
            ChamadoEm = null;
            FinalizadoEm = null;
        }

        public void ChamarParaAtendimento()
        {
            Status = StatusAtendimento.EmAtendimento;
            ChamadoEm = DateTime.UtcNow;
            FinalizadoEm = null;
        }

        public void FinalizarAtendimento()
        {
            Status = StatusAtendimento.Finalizado;
            FinalizadoEm = DateTime.UtcNow;
        }
    }
}