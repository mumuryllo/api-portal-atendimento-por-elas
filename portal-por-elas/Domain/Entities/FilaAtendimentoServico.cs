namespace portal_por_elas.Domain.Entities
{
    public class FilaAtendimentoServico
    {
        protected FilaAtendimentoServico() { }

        public FilaAtendimentoServico(Guid filaAtendimentoId, Guid servicoId)
        {
            FilaAtendimentoId = filaAtendimentoId;
            ServicoId = servicoId;
        }

        public Guid FilaAtendimentoId { get; private set; }
        public FilaAtendimento FilaAtendimento { get; private set; } = null!;
        public Guid ServicoId { get; private set; }
        public Servico Servico { get; private set; } = null!;
    }
}