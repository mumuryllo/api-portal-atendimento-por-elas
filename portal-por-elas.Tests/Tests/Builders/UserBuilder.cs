using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Tests.Builders
{
    public class UserBuilder
    {
        private string _email = "mariana.fernandes@porelas.com";
        private bool _active = true;
        private string _name = "Mariana Fernandes";
        private string _passwordHash = BCrypt.Net.BCrypt.HashPassword("Senha@123");
        private string _role = AuthorizationConstants.Admin;
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiry;
        private DateTime? _tempPasswordExpiry;

        public UserBuilder ComEmail(string email) { _email = email; return this; }
        public UserBuilder ComNome(string name) { _name = name; return this; }
        public UserBuilder ComSenhaPlana(string senha) { _passwordHash = BCrypt.Net.BCrypt.HashPassword(senha); return this; }
        public UserBuilder ComoAdmin() { _role = AuthorizationConstants.Admin; return this; }
        public UserBuilder ComoMulher() { _role = AuthorizationConstants.Mulheres; return this; }
        public UserBuilder Inativo() { _active = false; return this; }

        public UserBuilder ComRefreshToken(string token, DateTime expiraEm)
        {
            _refreshToken = token;
            _refreshTokenExpiry = expiraEm;
            return this;
        }

        public UserBuilder ComSenhaTemporaria(string senhaPlana, DateTime expiraEm)
        {
            _passwordHash = BCrypt.Net.BCrypt.HashPassword(senhaPlana);
            _tempPasswordExpiry = expiraEm;
            return this;
        }

        public User Build()
        {
            var user = new User(_email, _active, _name, _passwordHash, _role);

            if (_tempPasswordExpiry.HasValue)
                user.SetTemporaryPassword(_passwordHash, _tempPasswordExpiry.Value);

            if (!string.IsNullOrWhiteSpace(_refreshToken) && _refreshTokenExpiry.HasValue)
                user.DefinirRefreshToken(_refreshToken, _refreshTokenExpiry.Value);

            return user;
        }

        public CreateUserDto BuildCreateDto(string senhaPlana = "Senha@123") => new()
        {
            Email = _email,
            Name = _name,
            Password = senhaPlana,
            Role = _role
        };

        public LoginRequestDto BuildLoginDto(string senhaPlana = "Senha@123") => new()
        {
            Email = _email,
            Password = senhaPlana
        };
    }
}