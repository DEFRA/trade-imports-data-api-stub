using System.Diagnostics.CodeAnalysis;

namespace Defra.TradeImportsDataApiStub.Stub;

[ExcludeFromCodeCoverage]
public static class EmbeddedStubData
{
    private static Type Anchor => typeof(EmbeddedStubData);

    private static string GetScenarioPrefix() => $"{Anchor.Namespace}.Scenarios.";

    public static IEnumerable<string> GetAllScenariosStubs() =>
        Anchor
            .Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith($"{GetScenarioPrefix()}"))
            .Select(x => x.Replace(GetScenarioPrefix(), ""));

    public static string GetBody(string fileName)
    {
        using var stream = Anchor.Assembly.GetManifestResourceStream(
            $"{GetScenarioPrefix()}{fileName}"
        );

        if (stream is null)
            throw new InvalidOperationException($"Unable to find embedded resource {fileName}");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
