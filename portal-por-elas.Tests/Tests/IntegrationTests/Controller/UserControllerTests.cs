using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Controllers;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Tests.Builders;

namespace portal_por_elas.Tests.UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly UserController _sut;

        public UserControllerTests()
        {
            _sut = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Create_QuandoDadosValidos_DeveRetornar201CreatedComUsuarioCriado()
        {
            // Arrange
            var dto = new UserBuilder()
                .ComEmail("mariana@porelas.com")
                .ComNome("Mariana Fernandes")
                .ComoMulher()
                .BuildCreateDto("Senha@123");

            var expectedResponse = new UserResponseDto
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role
            };

            _userServiceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Create(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdResult.Value.Should().BeEquivalentTo(expectedResponse);
            _userServiceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task UpdatePassword_QuandoSucesso_DeveRetornar204NoContent()
        {
            // Arrange
            var dto = PasswordRequestsMother.UpdateValido();

            // Act
            var result = await _sut.UpdatePassword(dto);

            // Assert
            var noContent = result.Should().BeOfType<NoContentResult>().Subject;
            noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _userServiceMock.Verify(s => s.UpdatePasswordAsync(dto), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_QuandoSucesso_DeveRetornar204NoContent()
        {
            // Arrange
            var dto = PasswordRequestsMother.ForgotValido("mariana@porelas.com");

            // Act
            var result = await _sut.ForgotPassword(dto);

            // Assert
            var noContent = result.Should().BeOfType<NoContentResult>().Subject;
            noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _userServiceMock.Verify(s => s.SendTemporaryPasswordAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Delete_QuandoSucesso_DeveRetornar204NoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _sut.Delete(userId);

            // Assert
            var noContent = result.Should().BeOfType<NoContentResult>().Subject;
            noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _userServiceMock.Verify(s => s.DeleteAsync(userId), Times.Once);
        }
    }
}