using FluentAssertions;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.Fila;
using portal_por_elas.Tests.Builders;
using portal_por_elas.Validators;
using portal_por_elas.Validators.Evento;
using portal_por_elas.Validators.Fila;

namespace portal_por_elas.Tests.UnitTests.Validators
{
    public class EventoEFilaValidatorsTests
    {
        [Fact]
        public void CriarEventoValidator_ComDadosValidos_DevePassar()
        {
            var validator = new CriarEventoRequestValidator();
            var dto = new EventoBuilder().BuildRequestDto();

            var result = validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void CriarEventoValidator_QuandoNomeVazioOuHoraFimMenorQueInicio_DeveFalhar()
        {
            var validator = new CriarEventoRequestValidator();
            var dto = new EventoBuilder()
                .ComNome("")
                .ComHorario(new TimeOnly(15, 0), new TimeOnly(10, 0))
                .BuildRequestDto();

            var result = validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public void CadastrarMulherFilaValidator_ComDadosValidos_DevePassar()
        {
            var validator = new CadastrarMulherFilaRequestValidator();
            var dto = new FilaAtendimentoBuilder()
                .ComServicos(ServicoMother.ManicureEPedicure().Id, ServicoMother.Maquiagem().Id)
                .BuildRequestDto();

            var result = validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void CadastrarMulherFilaValidator_QuandoEventoVazioNomeVazioOuSemServicos_DeveFalhar()
        {
            var validator = new CadastrarMulherFilaRequestValidator();
            var dto = new CadastrarMulherFilaRequestDto
            {
                EventoId = Guid.Empty,
                NomeMulher = "",
                ServicosIds = new List<Guid>()
            };

            var result = validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        }
    }
}