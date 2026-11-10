
using Microsoft.EntityFrameworkCore;
using Propostas.Domain.Entities;

namespace Propostas.Infrastructure.Persistence;

public sealed class PropostasDbContext : DbContext
{
    public PropostasDbContext(
        DbContextOptions<PropostasDbContext> options)
        : base(options)
    {
    }

    // Tabela de propostas de crédito
    public DbSet<Proposta> Propostas =>
        Set<Proposta>();

    // Tabela de eventos pendentes de publicação
    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================
        // ENTIDADE: PROPOSTA
        // =====================================

        modelBuilder.Entity<Proposta>(entity =>
        {
            entity.ToTable("Propostas");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.ClienteId)
                .IsRequired();

            // Evita propostas duplicadas por cliente
            entity.HasIndex(p => p.ClienteId)
                .IsUnique();

            entity.Property(p => p.Score)
                .IsRequired();

            entity.Property(p => p.LimiteCredito)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(p => p.QuantidadeCartoes)
                .IsRequired();

            entity.Property(p => p.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(p => p.CriadoEm)
                .IsRequired();
        });

        // =====================================
        // ENTIDADE: OUTBOX MESSAGE
        // =====================================

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Tipo)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(m => m.Conteudo)
                .IsRequired();

            entity.Property(m => m.CriadoEm)
                .IsRequired();

            entity.Property(m => m.ProcessadoEm);

            entity.Property(m => m.Tentativas)
                .IsRequired();

            entity.Property(m => m.UltimoErro)
                .HasMaxLength(4000);

            // Otimiza a busca de eventos pendentes
            entity.HasIndex(m => new
            {
                m.ProcessadoEm,
                m.CriadoEm
            });
        });
    }
}
