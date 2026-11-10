
namespace Propostas.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ClientesExchange { get; set; } =
        "paranabanco.clientes.events";

    public string ClientesQueue { get; set; } =
        "paranabanco.propostas.cliente-cadastrado";

    public string ClientesRoutingKey { get; set; } =
        "cliente.cadastrado";

    public string PropostasExchange { get; set; } =
        "paranabanco.propostas.events";

    public string PropostasRoutingKey { get; set; } =
        "proposta.gerada";

    public string RetryQueue { get; set; } =
    "paranabanco.propostas.cliente-cadastrado.retry";

    public string DeadLetterQueue { get; set; } =
        "paranabanco.propostas.cliente-cadastrado.dlq";

    public int MaxTentativas { get; set; } = 3;

    public int RetryDelayMilliseconds { get; set; } = 10000;
    public string RetryExchange { get; set; } =
        "paranabanco.propostas.retry";

    public string DeadLetterExchange { get; set; } =
        "paranabanco.propostas.dlx";

}
