using AutoMapper;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Exceptions;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Services;
using System.Security.Cryptography;

namespace portal_por_elas.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork uow, IMapper mapper, IEmailService emailService)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var existingUser = await _uow.Users.GetByEmailAsync(dto.Email);

            if (existingUser is not null)
                throw new ConflictException("Já existe um usuário cadastrado com este email.");

            var user = _mapper.Map<User>(dto);
            user.SetPassword(BCrypt.Net.BCrypt.HashPassword(dto.Password));
            user.SetActive(true);
            await _uow.Users.AddAsync(user);
            await _uow.CommitAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task SendTemporaryPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _uow.Users.GetByEmailAsync(normalizedEmail)
                ?? throw new NotFoundException("Usuário não encontrado com este e-mail.");

            if (!user.Active)
                throw new UnauthorizedException("Usuário inativo.");

            const int expirationMinutes = 15;
            var temporaryPassword = GenerateTemporaryPassword();
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

            user.SetTemporaryPassword(newPasswordHash, DateTime.UtcNow.AddMinutes(expirationMinutes));
            await _uow.Users.UpdateAsync(user);
            await _uow.CommitAsync();

            var emailBody = BuildPasswordResetTemplate(user.Name, temporaryPassword);

            await _emailService.SendEmailAsync(
                toEmail: user.Email,
                toName: user.Name,
                subject: "Portal Por Elas - Sua nova senha de acesso",
                htmlBody: emailBody
            );
        }
        public async Task UpdatePasswordAsync(UpdatePasswordRequestDto dto)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant())
                ?? throw new NotFoundException("Usuário não encontrado.");

            if (!user.Active)
                throw new UnauthorizedException("Usuário inativo.");

            if (user.IsTemporaryPasswordExpired())
                throw new UnauthorizedException("A senha temporária expirou. Solicite uma nova redefinição de senha.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedException("A senha temporária informada está incorreta.");

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

        private static string GenerateTemporaryPassword(int length = 10)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$%&*!";
            const string allChars = upper + lower + digits + special;

            var passwordChars = new char[length];
            passwordChars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            passwordChars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            passwordChars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            passwordChars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (int i = 4; i < length; i++)
            {
                passwordChars[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            RandomNumberGenerator.Shuffle(passwordChars.AsSpan());
            return new string(passwordChars);
        }

        private static string BuildPasswordResetTemplate(string userName, string temporaryPassword)
        {
            return $"""
                <div style="font-family: Arial, sans-serif; max-width: 520px; margin: 0 auto; color: #333;">
                    <h2 style="color: #6b21a8;">Portal Por Elas</h2>
                    <p>Olá, <strong>{userName}</strong>!</p>
                    <p>Recebemos uma solicitação de redefinição de senha para a sua conta.</p>
                    <p>Sua nova senha temporária de acesso é:</p>
                    <div style="background-color: #f3e8ff; border-left: 4px solid #6b21a8; padding: 12px 16px; font-size: 20px; font-weight: bold; letter-spacing: 1px; margin: 16px 0;">
                        {temporaryPassword}
                    </div>
                    <p>Recomendamos que você altere esta senha assim que realizar o login.</p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;" />
                    <small style="color: #777;">Caso você não tenha solicitado esta alteração, entre em contato com o suporte imediatamente.</small>
                </div>
                """;
        }
    }
}