using Propostas.Domain.Entities;

namespace Propostas.Application.Interfaces;

public interface IPropostaRepository
{
    Task<Proposta?> ObterPorClienteIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Proposta proposta,
        CancellationToken cancellationToken = default);
}
