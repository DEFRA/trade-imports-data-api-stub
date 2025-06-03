using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Defra.TradeImportsDataApiStub.Stub;

[ExcludeFromCodeCoverage]
public static partial class WireMockExtensions
{
    private static Type Anchor => typeof(WireMockExtensions);

    public static void StubHealth(this WireMockServer wireMock)
    {
        var response = Response.Create().WithStatusCode(StatusCodes.Status200OK);
        var request = Request.Create().WithPath("/health").UsingGet();

        wireMock.Given(request).RespondWith(response);
    }

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

    public static void StubUtilityUpdatesEndpoint<T>(
        this WireMockServer wireMock,
        ILogger<T> logger
    )
    {
        var scenarios = GetAllScenariosStubs();
        var updates = new List<Update>();

        foreach (var scenario in scenarios)
        {
            if (!IncludeInUpdatesEndpoint().IsMatch(scenario))
                continue;

            var jsonNode =
                JsonNode.Parse(GetBody(scenario)) ?? throw new InvalidOperationException();

            updates.Add(
                new Update(
                    jsonNode["importPreNotification"]!["referenceNumber"]!.GetValue<string>(),
                    jsonNode["updated"]!.GetValue<DateTime>()
                )
            );

            logger.LogInformation("Used {Scenario} for updates endpoint", scenario);
        }

        var response = Response
            .Create()
            .WithStatusCode(StatusCodes.Status200OK)
            .WithBodyAsJson(new Updates(updates.ToArray()), indented: true);
        var request = Request
            .Create()
            .WithPath("/utility/import-pre-notification-updates")
            .UsingGet();

        wireMock.Given(request).RespondWith(response);
    }

    public static void StubAllEndpointsAvailable(this WireMockServer wireMock)
    {
        var supportedEndpoints = GetAllScenariosStubs().Select(ConvertScenarioToEndpoint).ToList();

        wireMock
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(StatusCodes.Status200OK)
                    .WithBodyAsJson(new { supportedEndpoints }, indented: true)
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

    [GeneratedRegex(@"^.*\d{7}\.json$")]
    private static partial Regex IncludeInUpdatesEndpoint();

    private record Updates(
        [property: JsonProperty("importPreNotificationUpdates")]
            Update[] ImportPreNotificationUpdates
    );

    private record Update(
        [property: JsonProperty("referenceNumber")] string ReferenceNumber,
        [property: JsonProperty("updated")] DateTime Updated
    );
}
