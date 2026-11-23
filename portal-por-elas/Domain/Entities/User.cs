namespace portal_por_elas.Domain.Entities
{
    public class User
    {
        protected User() { }

        public User(string email, bool active, string name, string passwordHash, string role)
        {
            Id = Guid.NewGuid();
            SetEmail(email);
            SetPassword(passwordHash);
            SetName(name);
            SetActive(active);
            SetRole(role);
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool Active { get; private set; }
        public string Role { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime? TemporaryPasswordExpiryTime { get; private set; }

        public void SetTemporaryPassword(string temporaryPasswordHash, DateTime expiryTime)
        {
            SetPassword(temporaryPasswordHash);
            TemporaryPasswordExpiryTime = expiryTime;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-mail é obrigatório.");

            if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
                throw new ArgumentException("E-mail inválido.");

            Email = email.Trim().ToLowerInvariant();
        }

        public void SetPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Senha é obrigatória.");

            PasswordHash = passwordHash;
        }

        public void SetRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Perfil é obrigatório.");

            Role = role;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório.");

            Name = name.Trim();
        }

        public void SetActive(bool active)
        {
            Active = active;
        }

        public void ChangePassword(string newPasswordHash)
        {
            SetPassword(newPasswordHash);
            TemporaryPasswordExpiryTime = null;
        }
        public bool IsTemporaryPasswordExpired()
        {
            return TemporaryPasswordExpiryTime.HasValue && TemporaryPasswordExpiryTime.Value <= DateTime.UtcNow;
        }

        public void ChangeRole(string newRole)
        {
            SetRole(newRole);
        }
    }
}