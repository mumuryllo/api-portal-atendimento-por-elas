namespace portal_por_elas.Contracts.Response.Hateoas
{
    public class HateoasLinkResponseDto
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public bool RequiresAuth { get; set; }
        public List<string> AllowedRoles { get; set; } = new();

        // Exibido apenas em ambiente Development
        public string? ControllerAction { get; set; }
    }
}
