using AutoMapper;
using FluentAssertions;
using Moq;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Services;
using portal_por_elas.Tests.Builders;
using Xunit;

namespace portal_por_elas.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _sut = new UserService(_uowMock.Object, _mapperMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public async Task CreateAsync_QuandoEmailJaExiste_DeveLancarConflictException()
        {
            // Arrange
            var existingUser = new UserBuilder().ComEmail("duplicado@porelas.com").Build();
            var dto = new UserBuilder().ComEmail("duplicado@porelas.com").BuildCreateDto();

            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            // Act
            var act = async () => await _sut.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_QuandoSucesso_DeveHashearSenhaESalvar()
        {
            // Arrange
            var dto = new UserBuilder().ComEmail("nova@porelas.com").BuildCreateDto("SenhaForte@1");
            var mappedUser = new UserBuilder().ComEmail("nova@porelas.com").Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            _mapperMock.Setup(m => m.Map<User>(dto)).Returns(mappedUser);
            _mapperMock.Setup(m => m.Map<UserResponseDto>(mappedUser))
                .Returns(new UserResponseDto { Id = mappedUser.Id, Email = mappedUser.Email, Name = mappedUser.Name });

            // Act
            var response = await _sut.CreateAsync(dto);

            // Assert
            response.Email.Should().Be("nova@porelas.com");
            BCrypt.Net.BCrypt.Verify("SenhaForte@1", mappedUser.PasswordHash).Should().BeTrue();
            _userRepoMock.Verify(r => r.AddAsync(mappedUser), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SendTemporaryPasswordAsync_QuandoUsuarioAtivo_DeveGerarSenhaCom15MinutosEEnviarEmail()
        {
            // Arrange
            var user = new UserBuilder().ComEmail("muryllo@porelas.com").ComNome("Muryllo").Build();
            _userRepoMock.Setup(r => r.GetByEmailAsync("muryllo@porelas.com")).ReturnsAsync(user);

            // Act
            await _sut.SendTemporaryPasswordAsync(PasswordRequestsMother.ForgotValido("MURYLLO@porelas.com"));

            // Assert
            user.TemporaryPasswordExpiryTime.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync(
                "muryllo@porelas.com",
                "Muryllo",
                "Portal Por Elas - Sua nova senha de acesso",
                It.Is<string>(html => html.Contains("Portal Por Elas") && html.Contains("Muryllo"))),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_QuandoSenhaTemporariaExpirada_DeveLancarUnauthorizedException()
        {
            // Arrange
            var user = new UserBuilder()
                .ComEmail("mariana@porelas.com")
                .ComSenhaTemporaria("Temp@1234", DateTime.UtcNow.AddMinutes(-2))
                .Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync("mariana@porelas.com")).ReturnsAsync(user);

            // Act
            var act = async () => await _sut.UpdatePasswordAsync(PasswordRequestsMother.UpdateValido());

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("*expirou*");
        }

        [Fact]
        public async Task UpdatePasswordAsync_QuandoSenhaTemporariaIncorreta_DeveLancarUnauthorizedException()
        {
            // Arrange
            var user = new UserBuilder()
                .ComEmail("mariana@porelas.com")
                .ComSenhaTemporaria("SenhaCorreta@1", DateTime.UtcNow.AddMinutes(10))
                .Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync("mariana@porelas.com")).ReturnsAsync(user);

            // Act
            var act = async () => await _sut.UpdatePasswordAsync(
                PasswordRequestsMother.UpdateValido(currentPassword: "SenhaErrada@9"));

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("*incorreta*");
        }

        [Fact]
        public async Task UpdatePasswordAsync_QuandoValido_DeveAlterarSenhaELimparExpiracao()
        {
            // Arrange
            var user = new UserBuilder()
                .ComEmail("mariana@porelas.com")
                .ComSenhaTemporaria("Temp@1234", DateTime.UtcNow.AddMinutes(10))
                .Build();

            _userRepoMock.Setup(r => r.GetByEmailAsync("mariana@porelas.com")).ReturnsAsync(user);

            // Act
            await _sut.UpdatePasswordAsync(PasswordRequestsMother.UpdateValido(
                currentPassword: "Temp@1234",
                newPassword: "NovaSenhaDefinitiva@2026"));

            // Assert
            BCrypt.Net.BCrypt.Verify("NovaSenhaDefinitiva@2026", user.PasswordHash).Should().BeTrue();
            user.TemporaryPasswordExpiryTime.Should().BeNull();
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
        {
            var id = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

            var act = async () => await _sut.DeleteAsync(id);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}