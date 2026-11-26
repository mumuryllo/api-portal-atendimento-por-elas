using FluentAssertions;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Tests.Builders;
using Xunit;

namespace portal_por_elas.Tests.UnitTests.Domain
{
    public class UserEntityTests
    {
        [Fact]
        public void Construtor_ComDadosValidos_DeveCriarUsuarioENormalizarEmail()
        {
            // Arrange & Act
            var user = new UserBuilder()
                .ComEmail("  MARIANA.Silva@PoReLaS.COM  ")
                .ComNome("  Mariana Silva  ")
                .ComoMulher()
                .Build();

            // Assert
            user.Id.Should().NotBeEmpty();
            user.Email.Should().Be("mariana.silva@porelas.com");
            user.Name.Should().Be("Mariana Silva");
            user.Role.Should().Be(AuthorizationConstants.Mulheres);
            user.Active.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("email-sem-arroba")]
        public void SetEmail_QuandoInvalido_DeveLancarArgumentException(string emailInvalido)
        {
            // Arrange
            var builder = new UserBuilder().ComEmail(emailInvalido);

            // Act
            var act = () => builder.Build();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("", "Mariana", "hash", "ADMIN")]
        [InlineData("a@b.com", "", "hash", "ADMIN")]
        [InlineData("a@b.com", "Mariana", "", "ADMIN")]
        [InlineData("a@b.com", "Mariana", "hash", "")]
        public void Construtor_QuandoCamposObrigatoriosVazios_DeveLancarArgumentException(
            string email, string nome, string hash, string role)
        {
            var act = () => new portal_por_elas.Domain.Entities.User(email, true, nome, hash, role);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void IsTemporaryPasswordExpired_QuandoPrazoPassou_DeveRetornarTrue()
        {
            // Arrange
            var user = new UserBuilder()
                .ComSenhaTemporaria("Temp@123", DateTime.UtcNow.AddMinutes(-1))
                .Build();

            // Act & Assert
            user.IsTemporaryPasswordExpired().Should().BeTrue();
        }

        [Fact]
        public void IsTemporaryPasswordExpired_QuandoAindaNoPrazo_DeveRetornarFalse()
        {
            // Arrange
            var user = new UserBuilder()
                .ComSenhaTemporaria("Temp@123", DateTime.UtcNow.AddMinutes(15))
                .Build();

            // Act & Assert
            user.IsTemporaryPasswordExpired().Should().BeFalse();
        }

        [Fact]
        public void RefreshTokenValido_DeveValidarTokenCorrespondenteENaoExpirado()
        {
            // Arrange
            var user = new UserBuilder()
                .ComRefreshToken("token-correto", DateTime.UtcNow.AddDays(7))
                .Build();

            // Act & Assert
            user.RefreshTokenValido("token-correto").Should().BeTrue();
            user.RefreshTokenValido("token-errado").Should().BeFalse();
        }

        [Fact]
        public void ChangePassword_DeveAtualizarHash_LimparExpiracaoTemporaria_ERevogarRefreshToken()
        {
            // Arrange
            var user = new UserBuilder()
                .ComSenhaTemporaria("Temp@123", DateTime.UtcNow.AddMinutes(10))
                .ComRefreshToken("refresh-ativo", DateTime.UtcNow.AddDays(7))
                .Build();

            // Act
            user.ChangePassword("novo-hash-definitivo");

            // Assert
            user.PasswordHash.Should().Be("novo-hash-definitivo");
            user.TemporaryPasswordExpiryTime.Should().BeNull();
            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpiryTime.Should().BeNull();
        }
    }
}