using FluentValidation;
using portal_por_elas.Contracts.Request.Evento;

namespace portal_por_elas.Validators.Evento
{
    public class CriarEventoRequestValidator : AbstractValidator<CriarEventoRequestDto>
    {
        public CriarEventoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do evento é obrigatório.")
                .MaximumLength(150).WithMessage("O nome do evento deve ter no máximo 150 caracteres.");

            RuleFor(x => x.HoraFim)
                .GreaterThan(x => x.HoraInicio)
                .WithMessage("A hora final deve ser maior que a hora de início.");
        }
    }
}
