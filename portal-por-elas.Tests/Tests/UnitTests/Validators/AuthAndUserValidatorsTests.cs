using FluentAssertions;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;
using portal_por_elas.Tests.Builders;
using portal_por_elas.Validators;
using Xunit;

namespace portal_por_elas.Tests.UnitTests.Validators
{
    public class AuthAndUserValidatorsTests
    {
        [Fact]
        public void LoginValidator_ComEmailESenhaValidos_DevePassar()
        {
            var validator = new LoginRequestValidator();
            var dto = new UserBuilder().BuildLoginDto("Senha@123");

            var result = validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "123456")]
        [InlineData("email-invalido", "123456")]
        [InlineData("teste@porelas.com", "12345")] // Menos de 6 caracteres
        public void LoginValidator_ComDadosInvalidos_DeveFalhar(string email, string senha)
        {
            var validator = new LoginRequestValidator();
            var dto = new LoginRequestDto { Email = email, Password = senha };

            var result = validator.Validate(dto);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ForgotPasswordValidator_ComEmailAcimaDe150Caracteres_DeveFalhar()
        {
            var validator = new ForgotPasswordRequestValidator();
            var emailLongo = new string('a', 142) + "@test.com"; // 151 chars
            var dto = new ForgotPasswordRequestDto { Email = emailLongo };

            var result = validator.Validate(dto);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void UpdatePasswordValidator_QuandoNovaSenhaIgualASenhaTemporaria_DeveFalhar()
        {
            var validator = new UpdatePasswordRequestValidator();
            var dto = PasswordRequestsMother.UpdateValido(
                currentPassword: "SenhaRepetida@1",
                newPassword: "SenhaRepetida@1");

            var result = validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("diferente da senha temporária"));
        }

        [Fact]
        public void UpdatePasswordValidator_ComDadosValidos_DevePassar()
        {
            var validator = new UpdatePasswordRequestValidator();
            var dto = PasswordRequestsMother.UpdateValido();

            var result = validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }
    }
}