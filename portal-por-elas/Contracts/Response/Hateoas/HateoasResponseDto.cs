namespace portal_por_elas.Contracts.Response.Hateoas
{
    public class HateoasResponseDto
    {
        public string Environment { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public int TotalEndpoints { get; set; }
        public List<HateoasLinkResponseDto> Links { get; set; } = new();
    }
}
