
namespace Propostas.Domain.Entities;

public sealed class Proposta
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public int Score { get; private set; }
    public decimal LimiteCredito { get; private set; }
    public int QuantidadeCartoes { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    private Proposta()
    {
    }

    public Proposta(Guid clienteId, int score)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do cliente é obrigatório.",
                nameof(clienteId));

        if (score < 0 || score > 1000)
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "O score deve estar entre 0 e 1000.");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Score = score;
        CriadoEm = DateTime.UtcNow;

        if (score <= 100)
        {
            Status = "Negada";
            LimiteCredito = 0;
            QuantidadeCartoes = 0;
        }
        else if (score <= 500)
        {
            Status = "Aprovada";
            LimiteCredito = 1000m;
            QuantidadeCartoes = 1;
        }
        else
        {
            Status = "Aprovada";
            LimiteCredito = 5000m;
            QuantidadeCartoes = 2;
        }
    }
}
