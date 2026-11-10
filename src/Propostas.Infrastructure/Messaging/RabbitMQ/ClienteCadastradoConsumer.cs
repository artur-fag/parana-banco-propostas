
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Propostas.Application.Events;
using Propostas.Application.Handlers;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Propostas.Infrastructure.Messaging.RabbitMQ;

public sealed class ClienteCadastradoConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClienteCadastradoConsumer> _logger;
    private readonly RabbitMqOptions _options;

    public ClienteCadastradoConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ClienteCadastradoConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }


    private static int ObterTentativa(
        IReadOnlyBasicProperties propriedades)
    {
        if (propriedades.Headers is null ||
            !propriedades.Headers.TryGetValue(
                "x-retry-count",
                out var valor) ||
            valor is null)
        {
            return 1;
        }

        return valor switch
        {
            int numero => numero,
            long numero => checked((int)numero),
            byte[] bytes when int.TryParse(
                Encoding.UTF8.GetString(bytes),
                out var numero) => numero,
            _ => 1
        };
    }


    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true
        };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken: stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(
    exchange: _options.RetryExchange,
    type: ExchangeType.Direct,
    durable: true,
    cancellationToken: stoppingToken);


        await using var publishChannel =
            await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                stoppingToken);


        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _options.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = _options.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = _options.ClientesExchange,
                ["x-dead-letter-routing-key"] = _options.ClientesRoutingKey
            },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _options.RetryQueue,
            exchange: _options.RetryExchange,
            routingKey: "retry",
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _options.DeadLetterQueue,
            exchange: _options.DeadLetterExchange,
            routingKey: "failed",
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var evento = JsonSerializer.Deserialize<
                    ClienteCadastradoEvent>(args.Body.Span);

                if (evento is null)
                    throw new JsonException(
                        "Mensagem ClienteCadastrado inválida.");

                using var scope = _scopeFactory.CreateScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<
                        ProcessarClienteCadastradoHandler>();

                await handler.ProcessarAsync(
                    evento,
                    stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Cliente {ClienteId} processado.",
                    evento.ClienteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao processar ClienteCadastrado.");

                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _options.ClientesQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Consumidor RabbitMQ iniciado na fila {Queue}.",
            _options.ClientesQueue);

        await Task.Delay(
            Timeout.InfiniteTimeSpan,
            stoppingToken);
    }


}
