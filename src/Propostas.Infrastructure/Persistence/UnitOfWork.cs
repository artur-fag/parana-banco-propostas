
using Propostas.Application.Interfaces;

namespace Propostas.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PropostasDbContext _context;

    public UnitOfWork(PropostasDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
