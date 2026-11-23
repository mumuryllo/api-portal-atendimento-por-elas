using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Contracts.Response.Hateoas;
using portal_por_elas.Interfaces.Services;
using System.Security.Claims;

namespace portal_por_elas.Services
{
    public class HateoasService : IHateoasService
    {
        private readonly EndpointDataSource _endpointDataSource;
        private readonly IWebHostEnvironment _environment;
        private readonly string _baseUrl;

        public HateoasService(
            EndpointDataSource endpointDataSource,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _endpointDataSource = endpointDataSource;
            _environment = environment;
            _baseUrl = configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? (_environment.IsDevelopment()
                    ? "https://api.dev.portalporelas.com.br"
                    : "https://api.portalporelas.com.br");
        }

        public HateoasResponseDto DiscoverLinks(
            ClaimsPrincipal user,
            string? simulateRole = null)
        {
            var isDev = _environment.IsDevelopment();
            var isSimulation = isDev && !string.IsNullOrWhiteSpace(simulateRole);
            var isAuthenticated = (user.Identity?.IsAuthenticated ?? false) || isSimulation;

            var effectiveRole = isSimulation
                ? simulateRole!.Trim()
                : user.FindFirstValue(ClaimTypes.Role) ?? "Anonymous";

            var links = _endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .SelectMany(endpoint => MapEndpointToLinks(endpoint, isAuthenticated, effectiveRole, isDev))
                .ToList();

            links.AddRange(isDev ? GetDevelopmentLinks() : GetProductionLinks());

            return new HateoasResponseDto
            {
                Environment = _environment.EnvironmentName,
                CurrentRole = effectiveRole,
                TotalEndpoints = links.Count,
                Links = links.OrderBy(l => l.Rel).ToList()
            };
        }

        private IEnumerable<HateoasLinkResponseDto> MapEndpointToLinks(
            RouteEndpoint endpoint,
            bool isAuthenticated,
            string currentRole,
            bool isDev)
        {
            var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (action is null)
                yield break;

            var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
            var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            var requiresAuth = !allowAnonymous && authorizeData.Any();

            var allowedRoles = authorizeData
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requiresAuth && (!isAuthenticated || (allowedRoles.Any() && !allowedRoles.Contains(currentRole, StringComparer.OrdinalIgnoreCase))))
                yield break;

            var route = (endpoint.RoutePattern.RawText?.TrimStart('/') ?? string.Empty)
                .Replace("[controller]", action.ControllerName.ToLowerInvariant())
                .Replace("[action]", action.ActionName.ToLowerInvariant());

            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? new[] { "GET" };

            foreach (var method in methods)
            {
                yield return new HateoasLinkResponseDto
                {
                    Rel = action.ActionName.Equals("GetLinks", StringComparison.OrdinalIgnoreCase)
                        ? "self"
                        : $"{action.ControllerName.ToLowerInvariant()}-{action.ActionName.ToLowerInvariant()}-{method.ToLowerInvariant()}",
                    Href = $"{_baseUrl}/{route}",
                    Method = method,
                    RequiresAuth = requiresAuth,
                    AllowedRoles = allowedRoles.Any() ? allowedRoles : new List<string> { requiresAuth ? "Authenticated" : "Public" },
                    ControllerAction = isDev ? $"{action.ControllerName}Controller.{action.ActionName}" : null
                };
            }
        }

        private IEnumerable<HateoasLinkResponseDto> GetDevelopmentLinks() => new[]
        {
            new HateoasLinkResponseDto
            {
                Rel = "documentation-swagger",
                Href = $"{_baseUrl}/swagger/index.html",
                Method = "GET",
                RequiresAuth = false,
                AllowedRoles = new List<string> { "Public" },
                ControllerAction = "SwaggerUI"
            }
        };

        private IEnumerable<HateoasLinkResponseDto> GetProductionLinks() => new[]
        {
            new HateoasLinkResponseDto
            {
                Rel = "portal-frontend",
                Href = "https://portalporelas.com.br",
                Method = "GET",
                RequiresAuth = false,
                AllowedRoles = new List<string> { "Public" }
            }
        };
    }
}