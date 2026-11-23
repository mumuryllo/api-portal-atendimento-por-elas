namespace portal_por_elas.Domain.Constants
{
    public class DataBaseConstants
    {
        public static class Tables
        {
            public const string Users = "users";
        }

        public static class Columns
        {
            public const string Id = "id";

            // User
            public const string Name = "name";
            public const string Email = "email";
            public const string PasswordHash = "password_hash";
            public const string Active = "active";
            public const string Role = "role";
            public const string CreatedAt = "created_at";
            public const string TemporaryPasswordExpiryTime = "temporary_password_expiry_time";
        }
    }
}