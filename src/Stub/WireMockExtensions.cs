using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Defra.TradeImportsDataApiStub.Stub;

[ExcludeFromCodeCoverage]
public static class WireMockExtensions
{
    private static Type Anchor => typeof(WireMockExtensions);

    public static void StubAllScenarios<T>(this WireMockServer wireMock, ILogger<T> logger)
    {
        var scenarios = GetAllScenariosStubs();

        foreach (var scenario in scenarios)
        {
            var path = ConvertScenarioToEndpoint(scenario);
            var response = Response
                .Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithBody(GetBody(scenario));
            var request = Request.Create().WithPath(path).UsingGet();

            wireMock.Given(request).RespondWith(response);

            logger.LogInformation("Stubbed {Scenario}", path);
        }
    }

    public static void StubAllEndpointsAvailable(this WireMockServer wireMock)
    {
        wireMock
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(StatusCodes.Status200OK)
                    .WithBodyAsJson(
                        new
                        {
                            supportedEndpoints = GetAllScenariosStubs()
                                .Select(ConvertScenarioToEndpoint),
                        },
                        indented: true
                    )
            );
    }

    private static string ConvertScenarioToEndpoint(string scenario) =>
        scenario.Replace("_", "/").Replace(".json", "");

    private static IEnumerable<string> GetAllScenariosStubs() =>
        Anchor
            .Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith($"{GetScenarioPrefix()}"))
            .Select(x => x.Replace(GetScenarioPrefix(), ""));

    private static string GetBody(string fileName)
    {
        using var stream = Anchor.Assembly.GetManifestResourceStream(
            $"{GetScenarioPrefix()}{fileName}"
        );

        if (stream is null)
            throw new InvalidOperationException($"Unable to find embedded resource {fileName}");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static string GetScenarioPrefix() => $"{Anchor.Namespace}.Scenarios.";
}
