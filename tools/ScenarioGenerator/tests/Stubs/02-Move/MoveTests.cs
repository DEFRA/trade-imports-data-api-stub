namespace Defra.ScenarioGenerator.Tests.Stubs._02_Move;

public class MoveTests
{
    [Fact]
    public void MoveStubs()
    {
        const string path = "../../../../../../src/Stub/Scenarios/";
        var stubs = Directory.GetFiles("Stubs", "*.json");

        Directory
            .GetFiles(path)
            .ToList()
            .ForEach(x =>
            {
                if (x.EndsWith("_import-pre-notification-updates.json"))
                    return;
                File.Delete(x);
            });
        stubs.ToList().ForEach(x => File.Copy(x, $"{path}{Path.GetFileName(x)}"));
    }
}
