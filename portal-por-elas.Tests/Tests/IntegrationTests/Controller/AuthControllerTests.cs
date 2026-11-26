using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Controllers;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Tests.Builders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace portal_por_elas.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Login_QuandoCredenciaisValidas_DeveRetornar200OkComTokens()
        {
            // Arrange
            var request = new UserBuilder().BuildLoginDto("Admin@123");
            var expectedResponse = new LoginResponseDto
            {
                AccessToken = "jwt-valido",
                RefreshToken = "refresh-valido",
                AccessTokenExpiraEm = DateTime.UtcNow.AddMinutes(30),
                Name = "Mariana Fernandes",
                Email = request.Email,
                Role = AuthorizationConstants.Admin
            };

            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Login(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Refresh_QuandoTokensValidos_DeveRetornar200OkComNovosTokens()
        {
            // Arrange
            var request = new RefreshTokenRequestDto
            {
                AccessToken = "jwt-expirado",
                RefreshToken = "refresh-valido"
            };
            var expectedResponse = new LoginResponseDto
            {
                AccessToken = "novo-jwt",
                RefreshToken = "novo-refresh"
            };

            _authServiceMock.Setup(s => s.RefreshTokenAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Refresh(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Revoke_QuandoUsuarioAutenticadoComSubValido_DeveRevogarERetornar204NoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
                    }, "Bearer"))
                }
            };

            // Act
            var result = await _sut.Revoke();

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _authServiceMock.Verify(s => s.RevokeAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Revoke_QuandoClaimDeUsuarioAusenteOuInvalida_DeveRetornar401Unauthorized()
        {
            // Arrange
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = ClaimsPrincipalMother.Anonimo()
                }
            };

            // Act
            var result = await _sut.Revoke();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
            _authServiceMock.Verify(s => s.RevokeAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}