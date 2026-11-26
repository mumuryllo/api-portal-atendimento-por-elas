using portal_por_elas.Contracts.Request.Fila;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Tests.Builders
{
    public class FilaAtendimentoBuilder
    {
        private Guid _eventoId = Guid.NewGuid();
        private string _nomeMulher = "Ana Clara Souza";
        private List<Guid> _servicosIds = new()
        {
            ServicoMother.ManicureEPedicure().Id
        };

        public FilaAtendimentoBuilder ParaEvento(Guid eventoId)
        {
            _eventoId = eventoId;
            return this;
        }

        public FilaAtendimentoBuilder ComNomeMulher(string nome)
        {
            _nomeMulher = nome;
            return this;
        }

        public FilaAtendimentoBuilder ComServicos(params Guid[] ids)
        {
            _servicosIds = ids.ToList();
            return this;
        }

        public FilaAtendimento Build() => new(_eventoId, _nomeMulher, _servicosIds);

        public CadastrarMulherFilaRequestDto BuildRequestDto() => new()
        {
            EventoId = _eventoId,
            NomeMulher = _nomeMulher,
            ServicosIds = _servicosIds
        };
    }
}