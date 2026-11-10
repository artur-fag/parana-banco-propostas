using Propostas.Infrastructure.Messaging.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propostas.Application.Interfaces;
using Propostas.Infrastructure.Persistence;
using Propostas.Infrastructure.Persistence.Repositories;

namespace Propostas.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PropostasDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'PropostasDb' não foi configurada.");
        }

        services.AddDbContext<PropostasDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPropostaRepository, PropostaRepository>();

        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<RabbitMqOptions>(
    configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddHostedService<ClienteCadastradoConsumer>();

        services.AddSingleton<PropostaGeradaPublisher>();

        services.AddHostedService<PropostasOutboxWorker>();

        return services;
    }
}
