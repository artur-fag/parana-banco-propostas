
using Microsoft.EntityFrameworkCore;
using Propostas.Application.Interfaces;
using Propostas.Domain.Entities;

namespace Propostas.Infrastructure.Persistence.Repositories;

public sealed class PropostaRepository
    : IPropostaRepository
{
    private readonly PropostasDbContext _context;

    public PropostaRepository(
        PropostasDbContext context)
    {
        _context = context;
    }

    public async Task<Proposta?> ObterPorClienteIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Propostas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.ClienteId == clienteId,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Proposta proposta,
        CancellationToken cancellationToken = default)
    {
        await _context.Propostas.AddAsync(
            proposta,
            cancellationToken);
    }
}
