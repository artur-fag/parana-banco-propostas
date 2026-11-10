
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Propostas.Application.Interfaces;

namespace Propostas.Infrastructure.Messaging.RabbitMQ;

public sealed class PropostasOutboxWorker(
    IServiceScopeFactory scopeFactory,
    PropostaGeradaPublisher publisher,
    ILogger<PropostasOutboxWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var outbox = scope.ServiceProvider
                    .GetRequiredService<IOutboxRepository>();

                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                var pendentes = await outbox.ObterPendentesAsync(
                    20,
                    stoppingToken);

                foreach (var mensagem in pendentes)
                {
                    try
                    {
                        await publisher.PublicarAsync(
                            mensagem.Conteudo,
                            stoppingToken);

                        mensagem.MarcarComoProcessado();

                        await unitOfWork.SaveChangesAsync(
                            stoppingToken);

                        logger.LogInformation(
                            "Evento Outbox {Id} publicado.",
                            mensagem.Id);
                    }
                    catch (Exception ex)
                    {
                        mensagem.RegistrarFalha(ex.Message);

                        await unitOfWork.SaveChangesAsync(
                            stoppingToken);

                        logger.LogError(
                            ex,
                            "Erro ao publicar evento Outbox {Id}.",
                            mensagem.Id);
                    }
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro no processamento do Outbox.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(
                    stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
