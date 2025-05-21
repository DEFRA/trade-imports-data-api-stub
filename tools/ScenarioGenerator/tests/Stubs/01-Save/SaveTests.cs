using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace Defra.ScenarioGenerator.Tests.Stubs._01_Save;

public class SaveTests
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    [Fact]
    public async Task SaveStubs()
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:8081") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String("Developer:developer-pwd"u8.ToArray())
        );

        const string path = "../../../Scenarios/IPAFFS/";
        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        var cheds = files
            .Select(x =>
                Path.GetFileNameWithoutExtension(x).Split("-").First().Replace("_", ".").ToUpper()
            )
            .ToList();

        Directory.CreateDirectory("Stubs");
        Directory.GetFiles("Stubs").ToList().ForEach(File.Delete);

        const string created = "2025-02-21T13:28:39.129Z";
        const string updated = "2025-02-21T13:28:40.129Z";
        const string timestamp = "2025-02-21T13:28:41.129Z";

        foreach (var ched in cheds)
        {
            var uri = $"/import-pre-notifications/{ched}";
            var json = await GetDocument(client, uri);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var jsonNode = JsonNode.Parse(json)!;

                AssertPropertyAndUpdate(jsonNode, ["created"], created);
                AssertPropertyAndUpdate(jsonNode, ["updated"], updated);
                AssertPropertyAndUpdate(
                    jsonNode,
                    ["importPreNotification", "partOne", "pointOfEntry"],
                    "GBTEEP1"
                );

                await File.WriteAllTextAsync(
                    $"Stubs/{uri.Replace("/", "_")}.json",
                    jsonNode.ToJsonString(s_jsonOptions)
                );
            }

            uri = $"/import-pre-notifications/{ched}/customs-declarations";
            json = await GetDocument(client, uri);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var jsonNode = JsonNode.Parse(json)!;

                foreach (var node in jsonNode["customsDeclarations"]!.AsArray())
                {
                    AssertPropertyAndUpdate(node!, ["created"], created);
                    AssertPropertyAndUpdate(node!, ["updated"], updated);
                    AssertPropertyAndUpdate(node!, ["clearanceDecision", "timestamp"], timestamp);
                }

                await File.WriteAllTextAsync(
                    $"Stubs/{uri.Replace("/", "_")}.json",
                    jsonNode.ToJsonString(s_jsonOptions)
                );
            }
            
            uri = $"/import-pre-notifications/{ched}/gmrs";
            json = await GetDocument(client, uri);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var jsonNode = JsonNode.Parse(json)!;

                foreach (var node in jsonNode["gmrs"]!.AsArray())
                {
                    AssertPropertyAndUpdate(node!, ["created"], created);
                    AssertPropertyAndUpdate(node!, ["updated"], updated);
                }

                await File.WriteAllTextAsync(
                    $"Stubs/{uri.Replace("/", "_")}.json",
                    jsonNode.ToJsonString(s_jsonOptions)
                );
            }
        }
    }

    private static async Task<string> GetDocument(HttpClient client, string path)
    {
        var response = await client.GetStringAsync(path);
        var document = JsonDocument.Parse(response);

        return JsonSerializer.Serialize(document, s_jsonOptions);
    }

    private static void AssertPropertyAndUpdate(
        JsonNode jsonNode,
        IEnumerable<string> nestedPropertyNames,
        string value
    )
    {
        var activeJsonNode = jsonNode;
        var propertyNames = nestedPropertyNames.ToArray();

        foreach (var propertyName in propertyNames.SkipLast(1))
        {
            activeJsonNode[propertyName].Should().NotBeNull();
            activeJsonNode = activeJsonNode[propertyName];
            activeJsonNode.Should().NotBeNull();
        }

        var lastPropertyName = propertyNames[^1];

        activeJsonNode[lastPropertyName]
            .Should()
            .NotBeNull($"Field chain {string.Join(':', propertyNames)} not found");
        activeJsonNode[lastPropertyName] = value;
    }
}
