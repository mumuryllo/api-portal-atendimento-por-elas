using FluentAssertions;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Domain.Enums;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Domain
{
    public class FilaAtendimentoEntityTests
    {
        [Fact]
        public void Construtor_ComMultiplosServicos_DeveIniciarAguardandoEVincularServicosSemDuplicidade()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var manicureId = ServicoMother.ManicureEPedicure().Id;
            var corteId = ServicoMother.CorteEEscova().Id;

            // Act (passando manicureId duplicado propositalmente para validar deduplicação)
            var atendimento = new FilaAtendimentoBuilder()
                .ParaEvento(eventoId)
                .ComNomeMulher("  Camila Oliveira  ")
                .ComServicos(manicureId, corteId, manicureId)
                .Build();

            // Assert
            atendimento.Id.Should().NotBeEmpty();
            atendimento.EventoId.Should().Be(eventoId);
            atendimento.NomeMulher.Should().Be("Camila Oliveira");
            atendimento.Status.Should().Be(StatusAtendimento.Aguardando);
            atendimento.ChamadoEm.Should().BeNull();
            atendimento.FinalizadoEm.Should().BeNull();
            atendimento.Servicos.Should().HaveCount(2);
            atendimento.Servicos.Select(s => s.ServicoId).Should().BeEquivalentTo(new[] { manicureId, corteId });
            atendimento.Servicos.Should().OnlyContain(s => s.FilaAtendimentoId == atendimento.Id);
        }

        [Fact]
        public void Construtor_QuandoEventoIdEmpty_DeveLancarArgumentException()
        {
            var builder = new FilaAtendimentoBuilder().ParaEvento(Guid.Empty);

            var act = () => builder.Build();

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Construtor_QuandoNomeMulherVazio_DeveLancarArgumentException(string nomeInvalido)
        {
            var builder = new FilaAtendimentoBuilder().ComNomeMulher(nomeInvalido);

            var act = () => builder.Build();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Construtor_QuandoListaDeServicosVazia_DeveLancarArgumentException()
        {
            var builder = new FilaAtendimentoBuilder().ComServicos();

            var act = () => builder.Build();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FluxoCompletoDeStatus_Aguardando_EmAtendimento_VoltarParaAguardando_Finalizado()
        {
            // Arrange
            var atendimento = new FilaAtendimentoBuilder().Build();
            atendimento.Status.Should().Be(StatusAtendimento.Aguardando);

            // Act 1: Chamar para atendimento
            atendimento.ChamarParaAtendimento();

            // Assert 1
            atendimento.Status.Should().Be(StatusAtendimento.EmAtendimento);
            atendimento.ChamadoEm.Should().NotBeNull();
            atendimento.FinalizadoEm.Should().BeNull();

            // Act 2: Devolver para a fila (ex: mulher não estava presente no momento)
            atendimento.ColocarEmEspera();

            // Assert 2
            atendimento.Status.Should().Be(StatusAtendimento.Aguardando);
            atendimento.ChamadoEm.Should().BeNull();
            atendimento.FinalizadoEm.Should().BeNull();

            // Act 3: Chamar novamente e finalizar
            atendimento.ChamarParaAtendimento();
            atendimento.FinalizarAtendimento();

            // Assert 3
            atendimento.Status.Should().Be(StatusAtendimento.Finalizado);
            atendimento.ChamadoEm.Should().NotBeNull();
            atendimento.FinalizadoEm.Should().NotBeNull();
        }

        [Fact]
        public void FilaAtendimentoServico_QuandoIdsInvalidos_DeveLancarArgumentException()
        {
            var actFilaVazia = () => new FilaAtendimentoServico(Guid.Empty, Guid.NewGuid());
            var actServicoVazio = () => new FilaAtendimentoServico(Guid.NewGuid(), Guid.Empty);

            actFilaVazia.Should().Throw<ArgumentException>();
            actServicoVazio.Should().Throw<ArgumentException>();
        }
    }
}