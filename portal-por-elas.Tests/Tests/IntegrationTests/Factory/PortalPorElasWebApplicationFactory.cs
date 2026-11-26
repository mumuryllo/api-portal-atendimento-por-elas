using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using portal_por_elas.Data.Context;

namespace portal_por_elas.Tests.IntegrationTests
{
    public class PortalPorElasWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"PortalPorElasTestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Define ambiente isolado para não executar migrations relacionais (PostgreSQL) no startup
            builder.UseEnvironment("Testing");

            // Garante configurações válidas para JWT e API nos testes de integração
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "ChaveSuperSecretaDeTestesDeIntegracaoCom32Chars!",
                    ["JwtSettings:Issuer"] = "PortalPorElasAPI",
                    ["JwtSettings:Audience"] = "PortalPorElasClient",
                    ["ApiSettings:BaseUrl"] = "http://localhost"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove todas as configurações anteriores do DbContext (Npgsql / PostgreSQL)
                services.RemoveAll<DbContextOptions<PortalPorElasDbContext>>();
                services.RemoveAll<PortalPorElasDbContext>();

                // Registra o banco em memória isolado para esta suíte de testes
                services.AddDbContext<PortalPorElasDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Cria o schema e popula o Seed inicial (ex: serviços de beleza)
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PortalPorElasDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}