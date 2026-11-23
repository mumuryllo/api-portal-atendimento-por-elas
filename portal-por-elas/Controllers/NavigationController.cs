namespace portal_por_elas.Controllers
{
    using global::portal_por_elas.Contracts.Response.Hateoas;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System.Security.Claims;

    namespace portal_por_elas.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Produces("application/json")]
        [Tags("HATEOAS & Navegação")]
        public class NavigationController : ControllerBase
        {
            private readonly EndpointDataSource _endpointDataSource;
            private readonly IWebHostEnvironment _environment;

            public NavigationController(
                EndpointDataSource endpointDataSource,
                IWebHostEnvironment environment)
            {
                _endpointDataSource = endpointDataSource;
                _environment = environment;
            }

            /// <summary>
            /// Retorna os links HATEOAS dos endpoints disponíveis de acordo com a role do usuário e o ambiente.
            /// </summary>
            /// <param name="simulateRole">
            /// (Apenas Development) Permite simular a visualização de links para uma role específica.
            /// </param>
            [HttpGet("links")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(HateoasResponseDto), StatusCodes.Status200OK)]
            public ActionResult<HateoasResponseDto> GetLinks([FromQuery] string? simulateRole = null)
            {
                var isDev = _environment.IsDevelopment();
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

                // Em Development permite simular role via query; em Production confia apenas no JWT
                var effectiveRole = isDev && !string.IsNullOrWhiteSpace(simulateRole)
                    ? simulateRole.Trim()
                    : User.FindFirstValue(ClaimTypes.Role) ?? "Anonymous";

                var isSimulation = isDev && !string.IsNullOrWhiteSpace(simulateRole);
                var effectiveAuthenticated = isAuthenticated || isSimulation;

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var links = new List<HateoasLinkResponseDto>();

                foreach (var endpoint in _endpointDataSource.Endpoints.OfType<RouteEndpoint>())
                {
                    var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    if (actionDescriptor is null)
                        continue;

                    var httpMethods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                        ?? new[] { "GET" };

                    var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
                    var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

                    var requiresAuth = !allowAnonymous && authorizeData.Any();

                    var allowedRoles = authorizeData
                        .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                        .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    // Regra de filtragem por autenticação e Role
                    if (!HasAccess(requiresAuth, effectiveAuthenticated, allowedRoles, effectiveRole))
                        continue;

                    var rawRoute = endpoint.RoutePattern.RawText?.TrimStart('/') ?? string.Empty;
                    var cleanRoute = rawRoute
                        .Replace("[controller]", actionDescriptor.ControllerName.ToLowerInvariant())
                        .Replace("[action]", actionDescriptor.ActionName.ToLowerInvariant());

                    foreach (var method in httpMethods)
                    {
                        links.Add(new HateoasLinkResponseDto
                        {
                            Rel = BuildRelName(actionDescriptor.ControllerName, actionDescriptor.ActionName, method),
                            Href = $"{baseUrl}/{cleanRoute}",
                            Method = method,
                            RequiresAuth = requiresAuth,
                            AllowedRoles = allowedRoles.Any()
                                ? allowedRoles
                                : (requiresAuth ? new List<string> { "Authenticated" } : new List<string> { "Public" }),
                            ControllerAction = isDev
                                ? $"{actionDescriptor.ControllerName}Controller.{actionDescriptor.ActionName}"
                                : null
                        });
                    }
                }

                // Em ambiente Development, adiciona também o atalho para a documentação Swagger
                if (isDev)
                {
                    links.Add(new HateoasLinkResponseDto
                    {
                        Rel = "documentation-swagger",
                        Href = $"{baseUrl}/swagger/index.html",
                        Method = "GET",
                        RequiresAuth = false,
                        AllowedRoles = new List<string> { "Public" },
                        ControllerAction = "SwaggerUI"
                    });
                }

                var response = new HateoasResponseDto
                {
                    Environment = _environment.EnvironmentName,
                    CurrentRole = effectiveRole,
                    TotalEndpoints = links.Count,
                    Links = links.OrderBy(l => l.Rel).ToList()
                };

                return Ok(response);
            }

            private static bool HasAccess(
                bool requiresAuth,
                bool isAuthenticated,
                List<string> allowedRoles,
                string currentRole)
            {
                if (!requiresAuth)
                    return true;

                if (!isAuthenticated)
                    return false;

                // Se exige apenas [Authorize] sem restrição de roles específicas
                if (!allowedRoles.Any())
                    return true;

                return allowedRoles.Contains(currentRole, StringComparer.OrdinalIgnoreCase);
            }

            private static string BuildRelName(string controller, string action, string httpMethod)
            {
                if (action.Equals("GetLinks", StringComparison.OrdinalIgnoreCase))
                    return "self";

                return $"{controller.ToLowerInvariant()}-{action.ToLowerInvariant()}-{httpMethod.ToLowerInvariant()}";
            }
        }
    }
}
