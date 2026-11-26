using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Data.UnitOfWork;
using portal_por_elas.Tests.Builders;
using Xunit;

namespace portal_por_elas.Tests.IntegrationTests.Data
{
    public class UserRepositoryAndUnitOfWorkTests
    {
        private static PortalPorElasDbContext CriarContextoIsolado()
        {
            var options = new DbContextOptionsBuilder<PortalPorElasDbContext>()
                .UseInMemoryDatabase($"DataLayerTestDb_{Guid.NewGuid()}")
                .Options;

            return new PortalPorElasDbContext(options);
        }

        [Fact]
        public async Task UnitOfWork_AddECommitAsync_DevePersistirUsuarioERecuperarPorEmail()
        {
            // Arrange
            using var context = CriarContextoIsolado();
            using var uow = new UnitOfWork(context);
            var user = new UserBuilder()
                .ComEmail("mariana.repo@porelas.com")
                .ComoMulher()
                .Build();

            // Act
            await uow.Users.AddAsync(user);
            var linhasAfetadas = await uow.CommitAsync();

            var encontrado = await uow.Users.GetByEmailAsync("mariana.repo@porelas.com");

            // Assert
            linhasAfetadas.Should().Be(1);
            encontrado.Should().NotBeNull();
            encontrado!.Id.Should().Be(user.Id);
            encontrado.Role.Should().Be("Mulheres");
        }

        [Fact]
        public async Task Repository_FindAsync_UpdateAsync_EDeleteAsync_DevemFuncionarComUnitOfWork()
        {
            // Arrange
            using var context = CriarContextoIsolado();
            using var uow = new UnitOfWork(context);

            var admin = new UserBuilder().ComEmail("admin@porelas.com").ComoAdmin().Build();
            var mulher = new UserBuilder().ComEmail("mulher@porelas.com").ComoMulher().Build();

            await uow.Users.AddAsync(admin);
            await uow.Users.AddAsync(mulher);
            await uow.CommitAsync();

            // Act 1: FindAsync filtrando por Role
            var apenasMulheres = await uow.Users.FindAsync(u => u.Role == "Mulheres");
            apenasMulheres.Should().ContainSingle(u => u.Email == "mulher@porelas.com");

            // Act 2: UpdateAsync
            mulher.ChangeRole("ADMIN");
            await uow.Users.UpdateAsync(mulher);
            await uow.CommitAsync();

            var atualizado = await uow.Users.GetByIdAsync(mulher.Id);
            atualizado!.Role.Should().Be("ADMIN");

            // Act 3: DeleteAsync
            await uow.Users.DeleteAsync(admin);
            await uow.CommitAsync();

            var todos = await uow.Users.GetAllAsync();
            todos.Should().ContainSingle();
        }
    }
}