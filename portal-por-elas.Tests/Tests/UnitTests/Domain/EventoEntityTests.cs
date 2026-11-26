using FluentAssertions;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Domain
{
    public class EventoEntityTests
    {
        [Fact]
        public void Construtor_ComDadosValidos_DeveCriarEventoENormalizarNome()
        {
            // Arrange & Act
            var evento = new EventoBuilder()
                .ComNome("  Mutirão Por Elas - Zona Leste  ")
                .ComData(new DateOnly(2026, 12, 15))
                .ComHorario(new TimeOnly(8, 30), new TimeOnly(16, 30))
                .Build();

            // Assert
            evento.Id.Should().NotBeEmpty();
            evento.Nome.Should().Be("Mutirão Por Elas - Zona Leste");
            evento.DataEvento.Should().Be(new DateOnly(2026, 12, 15));
            evento.HoraInicio.Should().Be(new TimeOnly(8, 30));
            evento.HoraFim.Should().Be(new TimeOnly(16, 30));
            evento.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Construtor_QuandoNomeVazioOuEspacos_DeveLancarArgumentException(string nomeInvalido)
        {
            // Arrange
            var builder = new EventoBuilder().ComNome(nomeInvalido);

            // Act
            var act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*nome*");
        }

        [Fact]
        public void Construtor_QuandoHoraFimMenorQueHoraInicio_DeveLancarArgumentException()
        {
            // Arrange
            var builder = new EventoBuilder()
                .ComHorario(new TimeOnly(14, 0), new TimeOnly(12, 0));

            // Act
            var act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*hora*");
        }

        [Fact]
        public void Construtor_QuandoHoraFimIgualHoraInicio_DeveLancarArgumentException()
        {
            // Arrange
            var horarioIgual = new TimeOnly(10, 0);
            var builder = new EventoBuilder()
                .ComHorario(horarioIgual, horarioIgual);

            // Act
            var act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}