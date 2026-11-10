
using Propostas.Domain.Entities;

namespace Propostas.UnitTests.Domain.Entities;

public sealed class PropostaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void ScoreAte100_DeveNegarProposta(int score)
    {
        var proposta = new Proposta(Guid.NewGuid(), score);

        Assert.Equal("Negada", proposta.Status);
        Assert.Equal(0m, proposta.LimiteCredito);
        Assert.Equal(0, proposta.QuantidadeCartoes);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public void ScoreEntre101E500_DeveAprovarUmCartao(int score)
    {
        var proposta = new Proposta(Guid.NewGuid(), score);

        Assert.Equal("Aprovada", proposta.Status);
        Assert.Equal(1000m, proposta.LimiteCredito);
        Assert.Equal(1, proposta.QuantidadeCartoes);
    }

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void ScoreAcimaDe500_DeveAprovarDoisCartoes(int score)
    {
        var proposta = new Proposta(Guid.NewGuid(), score);

        Assert.Equal("Aprovada", proposta.Status);
        Assert.Equal(5000m, proposta.LimiteCredito);
        Assert.Equal(2, proposta.QuantidadeCartoes);
    }

    [Fact]
    public void ClienteIdVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(
            () => new Proposta(Guid.Empty, 500));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void ScoreInvalido_DeveLancarExcecao(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Proposta(Guid.NewGuid(), score));
    }
}
