using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using portal_por_elas.Exceptions;
using portal_por_elas.Security;
using portal_por_elas.Tests.Builders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace portal_por_elas.Tests.IntegrationTests.Services.Security
{
    public class TokenServiceTests
    {
        private readonly JwtSettings _settings;
        private readonly TokenService _sut;

        public TokenServiceTests()
        {
            _settings = new JwtSettings
            {
                SecretKey = "ChaveSuperSecretaDeTestesComMaisDe32Caracteres!!",
                Issuer = "PortalPorElasAPI",
                Audience = "PortalPorElasClient"
            };
            _sut = new TokenService(Options.Create(_settings));
        }

        [Fact]
        public void GenerateToken_DeveGerarJwtComClaimsDeUsuarioERole()
        {
            // Arrange
            var user = new UserBuilder()
                .ComEmail("admin@porelas.com")
                .ComNome("Muryllo Admin")
                .ComoAdmin()
                .Build();

            // Act
            var (token, expiraEm) = _sut.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();
            expiraEm.Should().BeAfter(DateTime.UtcNow.AddMinutes(29));

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "admin@porelas.com");
            jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "ADMIN");
        }

        [Fact]
        public void GenerateRefreshToken_DeveGerarStringsAleatoriasDistintas()
        {
            var token1 = _sut.GenerateRefreshToken();
            var token2 = _sut.GenerateRefreshToken();

            token1.Should().NotBeNullOrWhiteSpace();
            token1.Should().NotBe(token2);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_QuandoTokenExpiradoMasAssinaturaValida_DeveRetornarClaims()
        {
            // Arrange: cria manualmente um JWT já expirado há 10 minutos
            var user = new UserBuilder().ComoMulher().Build();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var expiredToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) },
                expires: DateTime.UtcNow.AddMinutes(-10),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            ));

            // Act
            var principal = _sut.GetPrincipalFromExpiredToken(expiredToken);

            // Assert
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_QuandoAlgoritmoInvalido_DeveLancarUnauthorizedException()
        {
            // Arrange: token assinado com HmacSha512 em vez de HmacSha256
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey + _settings.SecretKey));
            var tokenAlgInvalido = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512)
            ));

            // Act
            var act = () => _sut.GetPrincipalFromExpiredToken(tokenAlgInvalido);

            // Assert
            act.Should().Throw<UnauthorizedException>();
        }
    }
}
