using FluentValidation;
using portal_por_elas.Contracts.Request.Fila;

namespace portal_por_elas.Validators.Fila
{
    public class CadastrarMulherFilaRequestValidator : AbstractValidator<CadastrarMulherFilaRequestDto>
    {
        public CadastrarMulherFilaRequestValidator()
        {
            RuleFor(x => x.EventoId)
                .NotEmpty().WithMessage("O evento é obrigatório.");

            RuleFor(x => x.NomeMulher)
                .NotEmpty().WithMessage("O nome da mulher é obrigatório.")
                .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.ServicosIds)
                .NotEmpty().WithMessage("Selecione ao menos um serviço.");
        }
    }
}
