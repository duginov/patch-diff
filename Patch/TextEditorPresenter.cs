using System.Diagnostics;

namespace Patch;

public class TextEditorPresenter : ConsoleSummaryPresenter
{
    public override void Present(DiffSummary summary)
    {
        var sb = PrepareTextOutput(summary);
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
        File.WriteAllText(tempFilePath, sb.ToString());

        Process.Start(new ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = tempFilePath,
            CreateNoWindow = true
        });

    }
}