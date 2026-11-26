using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Services;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Services
{
    public class HateoasServiceTests
    {
        private readonly FakeEndpointDataSource _endpointDataSource;

        public HateoasServiceTests()
        {
            var epSelf = new RouteEndpointBuilder()
                .ComRota("api/navigation/links")
                .ComControllerEAction("Navigation", "GetLinks")
                .ComMetodos("GET")
                .Publico()
                .Build();

            var epLogin = new RouteEndpointBuilder()
                .ComRota("api/auth/login")
                .ComControllerEAction("Auth", "Login")
                .ComMetodos("POST")
                .Publico()
                .Build();

            var epPainelMulheres = new RouteEndpointBuilder()
                .ComRota("api/fila/painel-mulheres")
                .ComControllerEAction("Fila", "ObterPainelMulheres")
                .ComMetodos("GET")
                .Autenticado($"{AuthorizationConstants.Mulheres},{AuthorizationConstants.Admin}")
                .Build();

            var epCriarEventoAdmin = new RouteEndpointBuilder()
                .ComRota("api/eventos")
                .ComControllerEAction("Eventos", "Criar")
                .ComMetodos("POST")
                .Autenticado(AuthorizationConstants.Admin)
                .Build();

            _endpointDataSource = new FakeEndpointDataSource(epSelf, epLogin, epPainelMulheres, epCriarEventoAdmin);
        }

        private HateoasService CriarService(string environmentName, string? baseUrl = "https://api.teste.com")
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns(environmentName);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiSettings:BaseUrl"] = baseUrl
                })
                .Build();

            return new HateoasService(_endpointDataSource, envMock.Object, config);
        }

        [Fact]
        public void DiscoverLinks_UsuarioAnonimoEmProduction_DeveRetornarApenasLinksPublicosEPortalFrontend()
        {
            // Arrange
            var sut = CriarService("Production");

            // Act
            var response = sut.DiscoverLinks(ClaimsPrincipalMother.Anonimo());

            // Assert
            response.CurrentRole.Should().Be("Anonymous");
            response.Links.Should().Contain(l => l.Rel == "self" && l.Href == "https://api.teste.com/api/navigation/links");
            response.Links.Should().Contain(l => l.Rel == "auth-login-post");
            response.Links.Should().Contain(l => l.Rel == "portal-frontend");
            response.Links.Should().NotContain(l => l.Rel == "documentation-swagger");
            response.Links.Should().NotContain(l => l.RequiresAuth);
            response.Links.Should().OnlyContain(l => l.ControllerAction == null);
        }

        [Fact]
        public void DiscoverLinks_UsuarioComRoleMulheres_DeveIncluirPainelMulheresEOcultarRotasExclusivasAdmin()
        {
            // Arrange
            var sut = CriarService("Production");
            var userMulher = ClaimsPrincipalMother.Autenticado(Guid.NewGuid(), AuthorizationConstants.Mulheres);

            // Act
            var response = sut.DiscoverLinks(userMulher);

            // Assert
            response.CurrentRole.Should().Be(AuthorizationConstants.Mulheres);
            response.Links.Should().Contain(l => l.Rel == "fila-obterpainelmulheres-get");
            response.Links.Should().NotContain(l => l.Rel == "eventos-criar-post");
        }

        [Fact]
        public void DiscoverLinks_EmProduction_DeveIgnorarSimulateRolePorSeguranca()
        {
            // Arrange
            var sut = CriarService("Production");

            // Act: usuário anônimo tenta simular ADMIN em produção
            var response = sut.DiscoverLinks(ClaimsPrincipalMother.Anonimo(), simulateRole: AuthorizationConstants.Admin);

            // Assert
            response.CurrentRole.Should().Be("Anonymous");
            response.Links.Should().NotContain(l => l.Rel == "eventos-criar-post");
        }

        [Fact]
        public void DiscoverLinks_EmDevelopmentComSimulateRoleAdmin_DeveRetornarTodosOsLinksESwagger()
        {
            // Arrange
            var sut = CriarService("Development");

            // Act
            var response = sut.DiscoverLinks(ClaimsPrincipalMother.Anonimo(), simulateRole: AuthorizationConstants.Admin);

            // Assert
            response.CurrentRole.Should().Be(AuthorizationConstants.Admin);
            response.Links.Should().Contain(l => l.Rel == "eventos-criar-post" && l.ControllerAction == "EventosController.Criar");
            response.Links.Should().Contain(l => l.Rel == "fila-obterpainelmulheres-get");
            response.Links.Should().Contain(l => l.Rel == "documentation-swagger");
        }
    }
}