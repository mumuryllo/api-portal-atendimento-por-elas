using FluentValidation;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;

namespace portal_por_elas.Validators
{
    public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequestDto>
    {
        public UpdatePasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Formato de e-mail inválido.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("A senha atual (enviada por e-mail) é obrigatória.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("A nova senha deve ter no mínimo 6 caracteres.")
                .NotEqual(x => x.CurrentPassword).WithMessage("A nova senha deve ser diferente da senha temporária.");
        }
    }
}