using FluentValidation;
using portal_por_elas.Contracts.Request.portal_por_elas.Contracts.Request;

namespace portal_por_elas.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Formato de e-mail inválido.")
                .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");
        }
    }
}
