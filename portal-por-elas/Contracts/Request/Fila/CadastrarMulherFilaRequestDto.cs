namespace portal_por_elas.Contracts.Request.Fila
{
    public class CadastrarMulherFilaRequestDto
    {
        public Guid EventoId { get; set; }
        public string NomeMulher { get; set; } = string.Empty;
        public List<Guid> ServicosIds { get; set; } = new();
    }
}