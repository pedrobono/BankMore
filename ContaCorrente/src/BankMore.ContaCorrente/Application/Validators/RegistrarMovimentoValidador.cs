using FluentValidation;
using BankMore.ContaCorrente.Application.Commands;

namespace BankMore.ContaCorrente.Application.Validators {
    public class RegistrarMovimentoValidator : AbstractValidator<RegistrarMovimentoCommand> {
        public RegistrarMovimentoValidator() {
            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da movimentação deve ser maior que zero.");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("O tipo de movimento (é obrigatório.")
                .Must(t => t == "C" || t == "D")
                .WithMessage("O tipo de movimento deve ser 'C' (Crédito) ou 'D' (Débito).");

            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("O RequestId é obrigatório para garantir a idempotência da transação.");
        }
    }
}
