namespace portal_por_elas.Domain.Entities
{
    public class Servico
    {
        protected Servico() { }

        public Servico(Guid id, string nome)
        {
            Id = id;
            DefinirNome(nome);
            Ativo = true;
        }

        public Servico(string nome) : this(Guid.NewGuid(), nome) { }

        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        private readonly List<FilaAtendimentoServico> _atendimentoServicos = new();
        public IReadOnlyCollection<FilaAtendimentoServico> AtendimentoServicos => _atendimentoServicos.AsReadOnly();

        public void DefinirNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do serviço é obrigatório.");

            Nome = nome.Trim();
        }
    }
}