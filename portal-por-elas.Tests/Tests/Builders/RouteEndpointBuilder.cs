using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System.Security.Claims;

namespace portal_por_elas.Tests.Builders
{
    public class RouteEndpointBuilder
    {
        private string _pattern = "api/teste/acao";
        private string _controllerName = "Teste";
        private string _actionName = "Acao";
        private string[] _httpMethods = new[] { "GET" };
        private bool _allowAnonymous;
        private bool _requiresAuth;
        private string? _roles;

        public RouteEndpointBuilder ComRota(string pattern) { _pattern = pattern; return this; }
        public RouteEndpointBuilder ComControllerEAction(string controller, string action) { _controllerName = controller; _actionName = action; return this; }
        public RouteEndpointBuilder ComMetodos(params string[] methods) { _httpMethods = methods; return this; }
        public RouteEndpointBuilder Publico() { _allowAnonymous = true; _requiresAuth = false; return this; }
        public RouteEndpointBuilder Autenticado(string? roles = null) { _allowAnonymous = false; _requiresAuth = true; _roles = roles; return this; }

        public RouteEndpoint Build()
        {
            var metadata = new List<object>
            {
                new ControllerActionDescriptor
                {
                    ControllerName = _controllerName,
                    ActionName = _actionName
                },
                new HttpMethodMetadata(_httpMethods)
            };

            if (_allowAnonymous)
                metadata.Add(new AllowAnonymousAttribute());

            if (_requiresAuth)
                metadata.Add(new AuthorizeAttribute { Roles = _roles });

            return new RouteEndpoint(
                requestDelegate: _ => Task.CompletedTask,
                routePattern: RoutePatternFactory.Parse(_pattern),
                order: 0,
                metadata: new EndpointMetadataCollection(metadata),
                displayName: $"{_controllerName}.{_actionName}"
            );
        }
    }

    public class FakeEndpointDataSource : EndpointDataSource
    {
        private readonly IReadOnlyList<Endpoint> _endpoints;
        public FakeEndpointDataSource(params Endpoint[] endpoints) => _endpoints = endpoints;
        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;
        public override Microsoft.Extensions.Primitives.IChangeToken GetChangeToken() =>
            Microsoft.Extensions.FileProviders.NullChangeToken.Singleton;
    }

    public static class ClaimsPrincipalMother
    {
        public static ClaimsPrincipal Anonimo() => new(new ClaimsIdentity());

        public static ClaimsPrincipal Autenticado(Guid userId, string role) =>
            new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, authenticationType: "TestAuth"));
    }
}