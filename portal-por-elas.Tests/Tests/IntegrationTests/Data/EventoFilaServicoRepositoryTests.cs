using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Data.UnitOfWork;
using portal_por_elas.Domain.Enums;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.IntegrationTests.Data
{
    public class EventoFilaServicoRepositoryTests
    {
        private static PortalPorElasDbContext CriarContextoIsolado()
        {
            var options = new DbContextOptionsBuilder<PortalPorElasDbContext>()
                .UseInMemoryDatabase($"EventoFilaRepoDb_{Guid.NewGuid()}")
                .Options;

            var context = new PortalPorElasDbContext(options);
            context.Database.EnsureCreated(); // Já executa o HasData (Seed dos serviços)
            return context;
        }

        [Fact]
        public async Task EventoRepository_DevePersistirERecuperarEventosOrdenados()
        {
            // Arrange
            using var context = CriarContextoIsolado();
            using var uow = new UnitOfWork(context);

            var evento1 = new EventoBuilder()
                .ComNome("Ação Social - Manhã")
                .ComData(new DateOnly(2026, 12, 5))
                .Build();

            var evento2 = new EventoBuilder()
                .ComNome("Ação Social - Tarde")
                .ComData(new DateOnly(2026, 12, 12))
                .Build();

            // Act
            await uow.Eventos.AddAsync(evento1);
            await uow.Eventos.AddAsync(evento2);
            await uow.CommitAsync();

            var todos = await uow.Eventos.GetAllAsync();

            // Assert
            todos.Should().HaveCount(2);
            todos.Should().Contain(e => e.Nome == "Ação Social - Manhã");
            todos.Should().Contain(e => e.Nome == "Ação Social - Tarde");
        }

        [Fact]
        public async Task ServicoRepository_DeveRetornarServicosDoSeedEAtivos()
        {
            // Arrange
            using var context = CriarContextoIsolado();
            using var uow = new UnitOfWork(context);

            var idManicureSeed = ServicoMother.ManicureEPedicure().Id;

            // Act
            var encontrado = await uow.Servicos.GetByIdAsync(idManicureSeed);
            var ativos = await uow.Servicos.FindAsync(s => s.Ativo);

            // Assert
            encontrado.Should().NotBeNull();
            encontrado!.Nome.Should().Be("Manicure e Pedicure");
            ativos.Should().NotBeEmpty();
        }

        [Fact]
        public async Task FilaAtendimentoRepository_DevePersistirAtendimentoComMultiplosServicosEAtualizarStatus()
        {
            // Arrange
            using var context = CriarContextoIsolado();
            using var uow = new UnitOfWork(context);

            var evento = new EventoBuilder().Build();
            await uow.Eventos.AddAsync(evento);

            // Usa os IDs dos serviços já inseridos pelo Seed no EnsureCreated()
            var servicosSeed = await uow.Servicos.GetAllAsync();
            var idsServicos = servicosSeed.Take(2).Select(s => s.Id).ToArray();

            var atendimento = new FilaAtendimentoBuilder()
                .ParaEvento(evento.Id)
                .ComNomeMulher("Fernanda Costa")
                .ComServicos(idsServicos)
                .Build();

            await uow.FilaAtendimentos.AddAsync(atendimento);
            await uow.CommitAsync();

            // Act 1: Recupera e verifica serviços associados
            var salvo = await uow.FilaAtendimentos.GetByIdAsync(atendimento.Id);
            salvo.Should().NotBeNull();
            salvo!.Servicos.Should().HaveCount(2);
            salvo.Status.Should().Be(StatusAtendimento.Aguardando);

            // Act 2: Transição de status para EmAtendimento
            salvo.ChamarParaAtendimento();
            await uow.FilaAtendimentos.UpdateAsync(salvo);
            await uow.CommitAsync();

            var atualizado = await uow.FilaAtendimentos.GetByIdAsync(atendimento.Id);
            atualizado!.Status.Should().Be(StatusAtendimento.EmAtendimento);
            atualizado.ChamadoEm.Should().NotBeNull();
        }
    }
}