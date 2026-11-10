
using Microsoft.EntityFrameworkCore;
using Propostas.Application.Interfaces;
using Propostas.Domain.Entities;

namespace Propostas.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly PropostasDbContext _context;

    public OutboxRepository(PropostasDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(
        OutboxMessage mensagem,
        CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(
            mensagem,
            cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> ObterPendentesAsync(
        int limite,
        CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(m => m.ProcessadoEm == null)
            .OrderBy(m => m.CriadoEm)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }
}
