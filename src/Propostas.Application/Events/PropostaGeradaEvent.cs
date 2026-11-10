namespace Propostas.Application.Events;

public sealed record PropostaGeradaEvent(
    Guid EventId,
    Guid PropostaId,
    Guid ClienteId,
    int Score,
    string Status,
    decimal LimiteCredito,
    int QuantidadeCartoes,
    DateTime OcorridoEm);
