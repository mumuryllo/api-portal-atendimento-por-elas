using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;

namespace portal_por_elas.Tests.Builders
{
    public static class PasswordRequestsMother
    {
        public static ForgotPasswordRequestDto ForgotValido(string email = "mariana@porelas.com") =>
            new() { Email = email };

        public static UpdatePasswordRequestDto UpdateValido(
            string email = "mariana@porelas.com",
            string currentPassword = "Temp@1234",
            string newPassword = "NovaSenha@2026") =>
            new()
            {
                Email = email,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };
    }
}