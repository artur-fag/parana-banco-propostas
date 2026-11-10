
using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Propostas.Infrastructure.Messaging.RabbitMQ;

public sealed class PropostaGeradaPublisher(
    IOptions<RabbitMqOptions> options)
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublicarAsync(
        string conteudo,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.PropostasExchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        // Garante uma fila durável para o futuro consumidor.
        const string queue =
            "paranabanco.cartoes.proposta-gerada";

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: _options.PropostasExchange,
            routingKey: _options.PropostasRoutingKey,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: _options.PropostasExchange,
            routingKey: _options.PropostasRoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(conteudo),
            cancellationToken: cancellationToken);
    }
}
