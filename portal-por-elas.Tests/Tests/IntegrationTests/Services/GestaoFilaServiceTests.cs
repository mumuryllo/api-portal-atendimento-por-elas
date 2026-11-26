using FluentAssertions;
using Moq;
using portal_por_elas.Contracts.Response.Fila;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Domain.Enums;
using portal_por_elas.Domain.Events;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Services;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Services
{
    public class GestaoFilaServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IEventoRepository> _eventoRepoMock = new();
        private readonly Mock<IServicoRepository> _servicoRepoMock = new();
        private readonly Mock<IFilaAtendimentoRepository> _filaRepoMock = new();
        private readonly Mock<IFilaNotificacaoService> _notificacaoMock = new();
        private readonly GestaoFilaService _sut;

        public GestaoFilaServiceTests()
        {
            _uowMock.Setup(u => u.Eventos).Returns(_eventoRepoMock.Object);
            _uowMock.Setup(u => u.Servicos).Returns(_servicoRepoMock.Object);
            _uowMock.Setup(u => u.FilaAtendimentos).Returns(_filaRepoMock.Object);

            _sut = new GestaoFilaService(_uowMock.Object, _notificacaoMock.Object);
        }

        [Fact]
        public async Task CriarEventoAsync_ComDadosValidos_DevePersistirERetornarDto()
        {
            // Arrange
            var dto = new EventoBuilder()
                .ComNome("Mutirão Por Elas - Itaquera")
                .BuildRequestDto();

            // Act
            var response = await _sut.CriarEventoAsync(dto);

            // Assert
            response.Id.Should().NotBeEmpty();
            response.Nome.Should().Be("Mutirão Por Elas - Itaquera");
            _eventoRepoMock.Verify(r => r.AddAsync(It.IsAny<Evento>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CadastrarMulherNaFilaAsync_QuandoEventoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            var dto = new FilaAtendimentoBuilder().BuildRequestDto();
            _eventoRepoMock.Setup(r => r.GetByIdAsync(dto.EventoId)).ReturnsAsync((Evento?)null);

            // Act
            var act = async () => await _sut.CadastrarMulherNaFilaAsync(dto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Evento não encontrado.");
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task CadastrarMulherNaFilaAsync_QuandoAlgumServicoInvalido_DeveLancarNotFoundException()
        {
            // Arrange
            var evento = new EventoBuilder().Build();
            var servicoValido = ServicoMother.ManicureEPedicure();
            var servicoInexistenteId = Guid.NewGuid();

            var dto = new FilaAtendimentoBuilder()
                .ParaEvento(evento.Id)
                .ComServicos(servicoValido.Id, servicoInexistenteId)
                .BuildRequestDto();

            _eventoRepoMock.Setup(r => r.GetByIdAsync(evento.Id)).ReturnsAsync(evento);
            _servicoRepoMock
                .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<Servico> { servicoValido }); // Retorna só 1 de 2

            // Act
            var act = async () => await _sut.CadastrarMulherNaFilaAsync(dto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Um ou mais serviços selecionados são inválidos.");
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task CadastrarMulherNaFilaAsync_QuandoDadosValidos_DevePersistirEnfileirarERetornarItem()
        {
            // Arrange
            var evento = new EventoBuilder().Build();
            var servico = ServicoMother.ManicureEPedicure();
            var dto = new FilaAtendimentoBuilder()
                .ParaEvento(evento.Id)
                .ComNomeMulher("Juliana Martins")
                .ComServicos(servico.Id)
                .BuildRequestDto();

            _eventoRepoMock.Setup(r => r.GetByIdAsync(evento.Id)).ReturnsAsync(evento);
            _servicoRepoMock
                .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<Servico> { servico });

            // Act
            var response = await _sut.CadastrarMulherNaFilaAsync(dto);

            // Assert
            response.NomeMulher.Should().Be("Juliana Martins");
            response.Status.Should().Be(StatusAtendimento.Aguardando);
            response.DescricaoStatus.Should().Be("Aguardando atendimento");
            response.Servicos.Should().ContainSingle().Which.Should().Be("Manicure e Pedicure");

            _filaRepoMock.Verify(r => r.AddAsync(It.IsAny<FilaAtendimento>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
            _notificacaoMock.Verify(n => n.Enfileirar(It.Is<ItemFilaAtendimentoResponseDto>(i => i.Id == response.Id)), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusAsync_QuandoAtendimentoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();
            _filaRepoMock.Setup(r => r.ObterPorIdComServicosAsync(idInexistente)).ReturnsAsync((FilaAtendimento?)null);

            // Act
            var act = async () => await _sut.AtualizarStatusAsync(idInexistente, StatusAtendimento.EmAtendimento);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Atendimento não encontrado na fila.");
        }

        [Fact]
        public async Task AtualizarStatusAsync_ParaEmAtendimento_DeveAtualizarStatusRemoverDaFilaENotificarChamada()
        {
            // Arrange
            var servico = ServicoMother.CorteEEscova();
            var atendimento = new FilaAtendimentoBuilder()
                .ComNomeMulher("Patrícia Rocha")
                .ComServicos(servico.Id)
                .Build();

            // Popula propriedade de navegação Servico usada no MapearAtendimento
            var itemServico = atendimento.Servicos.First();
            typeof(FilaAtendimentoServico)
                .GetProperty(nameof(FilaAtendimentoServico.Servico))!
                .SetValue(itemServico, servico);

            _filaRepoMock.Setup(r => r.ObterPorIdComServicosAsync(atendimento.Id)).ReturnsAsync(atendimento);

            // Act
            var response = await _sut.AtualizarStatusAsync(atendimento.Id, StatusAtendimento.EmAtendimento);

            // Assert
            response.Status.Should().Be(StatusAtendimento.EmAtendimento);
            response.DescricaoStatus.Should().Be("Atendida");
            atendimento.ChamadoEm.Should().NotBeNull();

            _filaRepoMock.Verify(r => r.UpdateAsync(atendimento), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
            _notificacaoMock.Verify(n => n.RemoverDaFila(atendimento.EventoId, atendimento.Id), Times.Once);
            _notificacaoMock.Verify(n => n.NotificarChamadaAsync(It.Is<ChamadaAtendimentoEventArgs>(e =>
                e.AtendimentoId == atendimento.Id &&
                e.NomeMulher == "Patrícia Rocha")), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusAsync_ParaAguardando_DeveColocarEmEsperaEReenfileirar()
        {
            // Arrange
            var servico = ServicoMother.Maquiagem();
            var atendimento = new FilaAtendimentoBuilder()
                .ComNomeMulher("Camila Dias")
                .ComServicos(servico.Id)
                .Build();
            atendimento.ChamarParaAtendimento();

            var itemServico = atendimento.Servicos.First();
            typeof(FilaAtendimentoServico)
                .GetProperty(nameof(FilaAtendimentoServico.Servico))!
                .SetValue(itemServico, servico);

            _filaRepoMock.Setup(r => r.ObterPorIdComServicosAsync(atendimento.Id)).ReturnsAsync(atendimento);

            // Act
            var response = await _sut.AtualizarStatusAsync(atendimento.Id, StatusAtendimento.Aguardando);

            // Assert
            response.Status.Should().Be(StatusAtendimento.Aguardando);
            response.DescricaoStatus.Should().Be("Aguardando atendimento");
            atendimento.ChamadoEm.Should().BeNull();
            _notificacaoMock.Verify(n => n.Enfileirar(It.Is<ItemFilaAtendimentoResponseDto>(i => i.Id == atendimento.Id)), Times.Once);
        }
    }
}