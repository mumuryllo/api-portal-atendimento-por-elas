using System.ComponentModel.DataAnnotations;

namespace portal_por_elas.Contracts.Request
{
    public class UpdatePasswordRequestDto
    {
  
        public string Email { get; set; } = string.Empty;


        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
