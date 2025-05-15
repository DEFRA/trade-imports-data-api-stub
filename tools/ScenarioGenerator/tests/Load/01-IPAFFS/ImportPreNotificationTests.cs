using Azure.Messaging.ServiceBus;
using Xunit.Abstractions;

namespace Defra.ScenarioGenerator.Tests.Load._01_IPAFFS;

public class ImportPreNotificationTests(ITestOutputHelper testOutputHelper)
    : ServiceBusTestBase("notification-topic", "btms")
{
    private const string Ched = ""; // "specific CHED to import";

    [Fact]
    public async Task LoadNotifications()
    {
        const string path = "../../../Scenarios/IPAFFS/";
        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var ched = Path.GetFileNameWithoutExtension(file)
                .Split("-")
                .First()
                .Replace("_", ".")
                .ToUpper();
            if (!string.IsNullOrWhiteSpace(Ched) && ched is not Ched)
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
