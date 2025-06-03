using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Defra.TradeImportsDataApiStub.Stub.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WireMock.Admin.Requests;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace Defra.TradeImportsDataApiStub.Stub;

[ExcludeFromCodeCoverage]
public class WireMockHostedService(
    IOptions<StubOptions> options,
    ILogger<WireMockHostedService> logger
) : IHostedService
{
    private readonly WireMockServerSettings _settings = new()
    {
        Port = options.Value.Port,
        Logger = new WireMockLogger(logger),
    };
    private WireMockServer? _wireMockServer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _wireMockServer = WireMockServer.Start(_settings);
        logger.LogInformation("Started on port {Port}", _settings.Port);

        ConfigureStubbedData();

        return Task.CompletedTask;
    }

    private void ConfigureStubbedData()
    {
        _wireMockServer?.StubHealth();
        _wireMockServer?.StubAllScenarios(logger);
        _wireMockServer?.StubUtilityUpdatesEndpoint(logger);
        _wireMockServer?.StubAllEndpointsAvailable();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_wireMockServer is not null)
        {
            _wireMockServer.Stop();
            logger.LogInformation("Stopped");
        }

        return Task.CompletedTask;
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    private sealed class WireMockLogger(ILogger<WireMockHostedService> logger) : IWireMockLogger
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
        };

        public void Debug(string formatString, params object[] args)
        {
            logger.LogDebug(formatString, args);
        }

        public void Info(string formatString, params object[] args)
        {
            logger.LogInformation(formatString, args);
        }

        public void Warn(string formatString, params object[] args)
        {
            logger.LogWarning(formatString, args);
        }

        public void Error(string formatString, params object[] args)
        {
            logger.LogError(formatString, args);
        }

        public void Error(string message, Exception exception)
        {
            logger.LogError(exception, message, exception);
        }

        public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
        {
            var message = JsonSerializer.Serialize(logEntryModel, _jsonSerializerOptions);
            logger.LogDebug("Admin[{IsAdminRequest}] {Message}", isAdminRequest, message);
        }
    }
}
