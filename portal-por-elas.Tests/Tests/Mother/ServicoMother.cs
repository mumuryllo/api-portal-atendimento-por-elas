using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Tests.Builders
{
    public static class ServicoMother
    {
        public static Servico ManicureEPedicure() =>
            new(Guid.Parse("11111111-1111-1111-1111-111111111101"), "Manicure e Pedicure");

        public static Servico CorteEEscova() =>
            new(Guid.Parse("11111111-1111-1111-1111-111111111102"), "Corte e Escova");

        public static Servico DesignSobrancelhas() =>
            new(Guid.Parse("11111111-1111-1111-1111-111111111103"), "Design de Sobrancelhas");

        public static Servico Maquiagem() =>
            new(Guid.Parse("11111111-1111-1111-1111-111111111104"), "Maquiagem");
    }
}