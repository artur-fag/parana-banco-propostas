
using System.Security.Cryptography;
using System.Text;

namespace Propostas.Application.Services;

public sealed class ScoreService
{
    public int Calcular(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException(
                "CPF é obrigatório.",
                nameof(cpf));

        var cpfNormalizado = new string(
            cpf.Where(char.IsDigit).ToArray());

        if (cpfNormalizado.Length != 11)
            throw new ArgumentException(
                "CPF deve conter 11 dígitos.",
                nameof(cpf));

        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(cpfNormalizado));

        var numero = BitConverter.ToUInt32(
            bytes, 0);

        return (int)(numero % 1001);
    }
}
