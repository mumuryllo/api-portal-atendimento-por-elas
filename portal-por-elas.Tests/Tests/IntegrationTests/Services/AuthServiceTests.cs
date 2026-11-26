using FluentAssertions;
using Moq;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Services;
using portal_por_elas.Tests.Builders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace portal_por_elas.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _sut = new AuthService(_uowMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarTokensESalvarRefreshToken()
        {
            // Arrange
            var user = new UserBuilder()
                .ComEmail("admin@porelas.com")
                .ComSenhaPlana("Admin@123")
                .ComoAdmin()
                .Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync("admin@porelas.com")).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns(("jwt-token", DateTime.UtcNow.AddMinutes(30)));
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token-gerado");

            // Act
            var result = await _sut.LoginAsync(new LoginRequestDto { Email = "ADMIN@porelas.com", Password = "Admin@123" });

            // Assert
            result.AccessToken.Should().Be("jwt-token");
            result.RefreshToken.Should().Be("refresh-token-gerado");
            user.RefreshToken.Should().Be("refresh-token-gerado");
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_QuandoUsuarioInativo_DeveLancarUnauthorizedException()
        {
            var user = new UserBuilder().ComEmail("inativo@porelas.com").ComSenhaPlana("123456").Inativo().Build();
            _userRepoMock.Setup(r => r.GetByEmailAsync("inativo@porelas.com")).ReturnsAsync(user);

            var act = async () => await _sut.LoginAsync(new LoginRequestDto { Email = "inativo@porelas.com", Password = "123456" });

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task LoginAsync_QuandoSenhaTemporariaExpirada_DeveLancarUnauthorizedException()
        {
            var user = new UserBuilder()
                .ComEmail("temp@porelas.com")
                .ComSenhaTemporaria("Temp@123", DateTime.UtcNow.AddMinutes(-5))
                .Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync("temp@porelas.com")).ReturnsAsync(user);

            var act = async () => await _sut.LoginAsync(new LoginRequestDto { Email = "temp@porelas.com", Password = "Temp@123" });

            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("*temporária expirou*");
        }

        [Fact]
        public async Task RefreshTokenAsync_QuandoValido_DeveRotacionarAccessERefreshToken()
        {
            // Arrange
            var user = new UserBuilder()
                .ComRefreshToken("refresh-antigo", DateTime.UtcNow.AddDays(5))
                .Build();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            }));

            _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken("jwt-expirado")).Returns(claimsPrincipal);
            _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns(("novo-jwt", DateTime.UtcNow.AddMinutes(30)));
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("novo-refresh");

            // Act
            var result = await _sut.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                AccessToken = "jwt-expirado",
                RefreshToken = "refresh-antigo"
            });

            // Assert
            result.AccessToken.Should().Be("novo-jwt");
            result.RefreshToken.Should().Be("novo-refresh");
            user.RefreshToken.Should().Be("novo-refresh");
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RevokeAsync_DeveLimparRefreshTokenDoUsuario()
        {
            // Arrange
            var user = new UserBuilder()
                .ComRefreshToken("token-ativo", DateTime.UtcNow.AddDays(5))
                .Build();

            _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            await _sut.RevokeAsync(user.Id);

            // Assert
            user.RefreshToken.Should().BeNull();
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}