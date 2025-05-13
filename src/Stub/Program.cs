using Defra.TradeImportsDataApiStub.Stub.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Defra.TradeImportsDataApiStub.Stub;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => ConfigureServices(services));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging => logging.AddConsole().AddDebug());

        services
            .AddOptions<StubOptions>()
            .BindConfiguration("Stub")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<WireMockHostedService>();
        services.AddHostedService<WireMockHostedService>(sp =>
            sp.GetRequiredService<WireMockHostedService>()
        );
    }
}
