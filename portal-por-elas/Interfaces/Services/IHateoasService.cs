using portal_por_elas.Contracts.Response;
using portal_por_elas.Contracts.Response.Hateoas;
using System.Security.Claims;

namespace portal_por_elas.Interfaces.Services
{
    public interface IHateoasService
    {
        HateoasResponseDto DiscoverLinks(ClaimsPrincipal user, string? simulateRole = null);
      
    }
}