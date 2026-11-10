
namespace Propostas.Domain.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid id,
        string tipo,
        string conteudo)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Identificador inválido.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException(
                "Tipo é obrigatório.",
                nameof(tipo));

        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException(
                "Conteúdo é obrigatório.",
                nameof(conteudo));

        Id = id;
        Tipo = tipo;
        Conteudo = conteudo;
        CriadoEm = DateTime.UtcNow;
    }

    public void MarcarComoProcessado()
    {
        ProcessadoEm = DateTime.UtcNow;
        UltimoErro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro;
    }
}
