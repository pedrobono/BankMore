using BankMore.ContaCorrente.Application.Commands;
using FluentValidation;

namespace BankMore.ContaCorrente.Application.Validators {
    public class CriarContaValidador : AbstractValidator<CriarContaCommand> {
        public CriarContaValidador() {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF é obrigatório")
                .Length(11).WithMessage("CPF deve ter 11 caracteres")
                .Matches(@"^\d+$").WithMessage("CPF deve conter apenas números");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

            RuleFor(x => x.NomeTitular)
                .NotEmpty().WithMessage("Nome do titular é obrigatório");
        }
    }
}
