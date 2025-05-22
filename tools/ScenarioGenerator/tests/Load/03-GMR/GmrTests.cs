using Azure.Messaging.ServiceBus;
using Xunit.Abstractions;

namespace Defra.ScenarioGenerator.Tests.Load._03_GMR;

public class GmrTests(ITestOutputHelper testOutputHelper)
    : ServiceBusTestBase(
        "defra.trade.dmp.outputgmrs.dev.1001.topic",
        "defra.trade.dmp.btms-ingest.dev.1001.subscription"
    )
{
    private const string GmrId = ""; // "specific GMR to import";

    [Fact]
    public async Task LoadGmrs()
    {
        const string path = "../../../Scenarios/GMR/";
        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var ched = Path.GetFileNameWithoutExtension(file)
                .Split("-")
                .First()
                .Replace("_", ".")
                .ToUpper();
            if (!string.IsNullOrWhiteSpace(GmrId) && ched is not GmrId)
            {
                testOutputHelper.WriteLine($"Skipping {ched}");
                continue;
            }

            var message = new ServiceBusMessage
            {
                Body = new BinaryData(await File.ReadAllTextAsync(file)),
            };

            await Sender.SendMessageAsync(message);
            testOutputHelper.WriteLine("Sent {0}", file);

            await Task.Delay(1000);
        }
    }
}
