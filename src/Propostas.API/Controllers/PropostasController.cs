using Microsoft.AspNetCore.Mvc;
using Propostas.Application.Interfaces;

namespace Propostas.API.Controllers;

[ApiController]
[Route("api/v1/propostas")]
public sealed class PropostasController : ControllerBase
{
    private readonly IPropostaRepository _repository;

    public PropostasController(IPropostaRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{clienteId:guid}")]
    public async Task<IActionResult> ObterPorClienteId(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        if (clienteId == Guid.Empty)
        {
            return BadRequest(new
            {
                mensagem = "ClienteId inválido."
            });
        }

        var proposta = await _repository.ObterPorClienteIdAsync(
            clienteId,
            cancellationToken);

        if (proposta is null)
        {
            return NotFound(new
            {
                mensagem = "Proposta não encontrada."
            });
        }

        return Ok(new
        {
            proposta.Id,
            proposta.ClienteId,
            proposta.Score,
            proposta.Status,
            proposta.LimiteCredito,
            proposta.QuantidadeCartoes,
            proposta.CriadoEm
        });
    }
}