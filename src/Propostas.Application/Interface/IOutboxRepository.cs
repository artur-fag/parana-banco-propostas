using Propostas.Domain.Entities;

namespace Propostas.Application.Interfaces;

public interface IOutboxRepository
{
    Task AdicionarAsync(
        OutboxMessage mensagem,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> ObterPendentesAsync(
        int limite,
        CancellationToken cancellationToken = default);
}
