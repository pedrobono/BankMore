using BankMore.Tarifa.Application.Commands;
using BankMore.Tarifa.Application.Handlers;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace BankMore.Tarifa.Tests.Unit;

public class ProcessarTarifacaoHandlerTests
{
    private readonly Mock<ITarifacaoRepository> _repositoryMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly IConfiguration _configurationMock;
    private readonly Mock<ILogger<ProcessarTarifacaoHandler>> _loggerMock;
    private readonly ProcessarTarifacaoHandler _handler;

    public ProcessarTarifacaoHandlerTests()
    {
        _repositoryMock = new Mock<ITarifacaoRepository>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _loggerMock = new Mock<ILogger<ProcessarTarifacaoHandler>>();

        var inMemorySettings = new Dictionary<string, string> {
            {"TarifaSettings:ValorTarifa", "2.00"}
        };
        _configurationMock = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _handler = new ProcessarTarifacaoHandler(
            _repositoryMock.Object,
            _kafkaProducerMock.Object,
            _configurationMock,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_DeveProcessarTarifacao_QuandoNaoExistir()
    {
        // Arrange
        var command = new ProcessarTarifacaoCommand("req-123", Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByRequestIdAsync(command.RequestId))
            .ReturnsAsync((Tarifacao?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Tarifacao>()), Times.Once);
        _kafkaProducerMock.Verify(k => k.ProduceAsync("tarifacoes-realizadas", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NaoDeveProcessar_QuandoJaExistir()
    {
        // Arrange
        var command = new ProcessarTarifacaoCommand("req-123", Guid.NewGuid());
        var tarifacaoExistente = new Tarifacao(command.IdContaOrigem, 2.00m, command.RequestId);
        _repositoryMock.Setup(r => r.GetByRequestIdAsync(command.RequestId))
            .ReturnsAsync(tarifacaoExistente);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Tarifacao>()), Times.Never);
        _kafkaProducerMock.Verify(k => k.ProduceAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveUsarValorConfigurado()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"TarifaSettings:ValorTarifa", "5.50"}
        };
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
        
        var handler = new ProcessarTarifacaoHandler(
            _repositoryMock.Object,
            _kafkaProducerMock.Object,
            customConfig,
            _loggerMock.Object);

        var command = new ProcessarTarifacaoCommand("req-456", Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByRequestIdAsync(command.RequestId))
            .ReturnsAsync((Tarifacao?)null);

        Tarifacao? tarifacaoCapturada = null;
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tarifacao>()))
            .Callback<Tarifacao>(t => tarifacaoCapturada = t)
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        tarifacaoCapturada.Should().NotBeNull();
        tarifacaoCapturada!.ValorTarifado.Should().Be(5.50m);
    }
}
