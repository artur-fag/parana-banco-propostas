
using System.Text.Json;
using Propostas.Application.Events;
using Propostas.Application.Interfaces;
using Propostas.Application.Services;
using Propostas.Domain.Entities;

namespace Propostas.Application.Handlers;

public sealed class ProcessarClienteCadastradoHandler
{
    private readonly IPropostaRepository _propostaRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ScoreService _scoreService;

    public ProcessarClienteCadastradoHandler(
        IPropostaRepository propostaRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        ScoreService scoreService)
    {
        _propostaRepository = propostaRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _scoreService = scoreService;
    }

    public async Task ProcessarAsync(
        ClienteCadastradoEvent evento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evento);

        if (evento.ClienteId == Guid.Empty)
            throw new ArgumentException(
                "ClienteId inválido.",
                nameof(evento));

        // Evita recriar uma proposta já existente.
        var propostaExistente =
            await _propostaRepository.ObterPorClienteIdAsync(
                evento.ClienteId,
                cancellationToken);

        if (propostaExistente is not null)
            return;

        // Calcula o score simulado.
        var score = _scoreService.Calcular(evento.Cpf);

        // Aplica as regras de crédito.
        var proposta = new Proposta(
            evento.ClienteId,
            score);

        // Prepara o evento de saída.
        var propostaGerada = new PropostaGeradaEvent(
            EventId: Guid.NewGuid(),
            PropostaId: proposta.Id,
            ClienteId: proposta.ClienteId,
            Score: proposta.Score,
            Status: proposta.Status,
            LimiteCredito: proposta.LimiteCredito,
            QuantidadeCartoes: proposta.QuantidadeCartoes,
            OcorridoEm: DateTime.UtcNow);

        var outboxMessage = new OutboxMessage(
            propostaGerada.EventId,
            nameof(PropostaGeradaEvent),
            JsonSerializer.Serialize(propostaGerada));

      
        await _propostaRepository.AdicionarAsync(
            proposta,
            cancellationToken);

        await _outboxRepository.AdicionarAsync(
            outboxMessage,
            cancellationToken);

   
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
