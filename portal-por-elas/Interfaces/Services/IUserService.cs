using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;

namespace portal_por_elas.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task DeleteAsync(Guid id);
        Task UpdatePasswordAsync(UpdatePasswordRequestDto dto);
    }
}
