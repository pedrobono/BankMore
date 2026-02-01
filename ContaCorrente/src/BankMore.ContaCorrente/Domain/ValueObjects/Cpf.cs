using System;
using System.Linq;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.Domain.ValueObjects {
    public class Cpf {
        public string Valor { get; private set; }

        public Cpf(string valor) {
            if (!EhValido(valor))
                throw new BusinessException("CPF inválido.", "INVALID_DOCUMENT");

            Valor = valor;
        }

        private bool EhValido(string cpf) {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            if (cpf.Distinct().Count() == 1)
                return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            tempCpf += primeiroDigito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith(primeiroDigito.ToString() + segundoDigito.ToString());
        }
    }
}
