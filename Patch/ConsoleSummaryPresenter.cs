using System.Text;

#pragma warning disable CA1860

namespace Patch
{
    /// <summary>
    /// Outputs comparison results to a console
    /// </summary>
    public class ConsoleSummaryPresenter : ISummaryPresenter
    {
        public virtual void Present(DiffSummary summary)
        {
            var sb = PrepareTextOutput(summary);
            Console.WriteLine(sb.ToString());
        }

        protected static StringBuilder PrepareTextOutput(DiffSummary summary)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Comparison result: {Path.GetFileName(summary.OldFile)} -> {Path.GetFileName(summary.NewFile)}\r\n");

            if (summary.ChangedLines.Any() || summary.NewLines.Any() || summary.RemovedLines.Any() || summary.RemovedFields.Any())
            {
                AppendRemovedFieldsInfo(summary, sb);
                AppendNewOverrides(summary, sb);
                AppendRemovedOverrides(summary, sb);
                AppendChangedOverrides(summary, sb);
            }
            else
            {
                sb.AppendLine("No changes found");
            }

            return sb;
        }

        protected static void AppendRemovedOverrides(DiffSummary summary, StringBuilder sb)
        {
            if (summary.RemovedLines.Any())
            {
                sb.AppendFormattedTitle("Overrides have been removed:");
                foreach (var line in summary.RemovedLines)
                {
                    sb.AppendLine($"For {summary.KeyName}: {line.Key}");
                    foreach (var value in line.Values)
                    {
                        sb.AppendLine($"\t{value.Key}: \t{value.Value}");
                    }
                }
            }
        }

        protected static void AppendChangedOverrides(DiffSummary summary, StringBuilder sb)
        {
            if (summary.ChangedLines.Any())
            {
                sb.AppendFormattedTitle("Modified overrides:");
                foreach (var line in summary.ChangedLines)
                {
                    sb.AppendLine($"For {summary.KeyName}: {line.Key}");
                    foreach (var valueDiff in line.ValueDiffs)
                    {
                        sb.AppendLine($"\t{valueDiff.Key}: \t{valueDiff.Value.ToString()}");
                    }
                }
            }
        }

        protected static void AppendNewOverrides(DiffSummary summary, StringBuilder sb)
        {
            if (summary.NewLines.Any())
            {
                sb.AppendFormattedTitle("New overrides:");
                foreach (var line in summary.NewLines)
                {
                    sb.AppendLine($"For {summary.KeyName}: {line.Key}");
                    foreach (var value in line.Values)
                    {
                        sb.AppendLine($"\t{value.Key}: \t{value.Value}");
                    }
                }
            }
        }

        protected static void AppendRemovedFieldsInfo(DiffSummary summary, StringBuilder sb)
        {
            if (summary.RemovedFields.Any())
            {
                sb.AppendFormattedTitle("Column(s) were removed:");
                foreach (var column in summary.RemovedFields)
                {
                    sb.AppendLine($"\t{column}");
                }
            }
        }

    }
}
