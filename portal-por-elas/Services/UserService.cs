using AutoMapper;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var existingUser = await _uow.Users.GetByEmailAsync(dto.Email);

            if (existingUser is not null)
                throw new ConflictException("Já existe um usuário cadastrado com este email.");

            var user = _mapper.Map<User>(dto);
            user.SetPassword(BCrypt.Net.BCrypt.HashPassword(dto.Password));
            await _uow.Users.AddAsync(user);
            await _uow.CommitAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

 

        public async Task UpdatePasswordAsync(UpdatePasswordRequestDto dto)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant())
                ?? throw new NotFoundException("Usuário não encontrado.");

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ChangePassword(newPasswordHash);

            await _uow.Users.UpdateAsync(user);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _uow.Users.GetByIdAsync(id)
                ?? throw new NotFoundException("Usuário não encontrado.");

            await _uow.Users.DeleteAsync(user);
            await _uow.CommitAsync();
        }
    }
}