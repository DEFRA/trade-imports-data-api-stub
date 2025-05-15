using Xunit.Abstractions;

namespace Defra.ScenarioGenerator.Tests.Load._02_CDS;

public class FinalisationTests(ITestOutputHelper testOutputHelper) : SqsTestBase(testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private const string Mrn = ""; // "specific MRN to import";

    [Fact]
    public async Task LoadFinalisations()
    {
        const string path = "../../../Scenarios/FINALISATION/";
        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var mrn = file.Replace(path, "").Split("-").First().ToUpper();
            if (!string.IsNullOrWhiteSpace(Mrn) && mrn is not Mrn)
            {
                _testOutputHelper.WriteLine($"Skipping {mrn}");
                continue;
            }

            await SendMessage(
                mrn,
                await File.ReadAllTextAsync(file),
                WithInboundHmrcMessageType(InboundHmrcMessageType.Finalisation)
            );

            await Task.Delay(1000);
        }
    }
}
