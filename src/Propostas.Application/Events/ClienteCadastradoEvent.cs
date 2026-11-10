namespace Propostas.Application.Events;

public sealed record ClienteCadastradoEvent(
    Guid EventId,
    Guid ClienteId,
    string Nome,
    string Cpf,
    string Email,
    DateTime OcorridoEm);
