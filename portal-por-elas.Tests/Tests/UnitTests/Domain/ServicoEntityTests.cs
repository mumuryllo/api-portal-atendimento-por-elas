using FluentAssertions;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Domain
{
    public class ServicoEntityTests
    {
        [Fact]
        public void Construtor_ComIdENomeValidos_DeveCriarServicoCorretamente()
        {
            // Arrange & Act
            var servico = ServicoMother.ManicureEPedicure();

            // Assert
            servico.Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111101"));
            servico.Nome.Should().Be("Manicure e Pedicure");
        }

        [Fact]
        public void Construtor_SemInformarId_DeveGerarNovoGuidValido()
        {
            // Arrange & Act
            var servico = new Servico("  Massagem Relaxante  ");

            // Assert
            servico.Id.Should().NotBeEmpty();
            servico.Nome.Should().Be("Massagem Relaxante");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Construtor_QuandoNomeVazio_DeveLancarArgumentException(string nomeInvalido)
        {
            // Act
            var act = () => new Servico(nomeInvalido);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Construtor_QuandoIdEmpty_DeveLancarArgumentException()
        {
            // Act
            var act = () => new Servico(Guid.Empty, "Corte de Cabelo");

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}