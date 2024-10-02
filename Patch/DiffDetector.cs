#pragma warning disable CA1860
namespace Patch
{
    public interface IDiffDetector
    {
        List<string> DetectRemovedFields(Patch oldPatch, Patch newPatch);
        List<string> DetectAddedColumns(Patch oldPatch, Patch newPatch);
        List<PatchLine> DetectUniqueLines(List<PatchLine> leftLines, List<PatchLine> rightLines);
        List<LineDiff> DetectChangesInLines(List<PatchLine> oldPatchLines, List<PatchLine> newPatchLines);
    }

    public class DiffDetector : IDiffDetector
    {
        public List<string> DetectRemovedFields(Patch oldPatch, Patch newPatch)
        {
            return oldPatch.ValueFields.Except(newPatch.ValueFields).ToList();
        }

        public List<string> DetectAddedColumns(Patch oldPatch, Patch newPatch)
        {
            return newPatch.ValueFields.Except(oldPatch.ValueFields).ToList();
        }

        public List<PatchLine> DetectUniqueLines(List<PatchLine> leftLines, List<PatchLine> rightLines)
        {
            var rightKeys = rightLines.Select(l => l.Key!);
            var removedLines = leftLines.Where(l => !rightKeys.Contains(l.Key!)).ToList();
            // we don't need null values for reporting - will remove them
            foreach (var patchLine in removedLines)
            {
                foreach (var key in patchLine.Values.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList())
                {
                    patchLine.Values.Remove(key);
                }
            }

            return removedLines;
        }

        public List<LineDiff> DetectChangesInLines(List<PatchLine> oldPatchLines, List<PatchLine> newPatchLines)
        {
            var lineDiffs = new List<LineDiff>();
            var oldKeys = oldPatchLines.Select(l => l.Key);
            var newKeys = newPatchLines.Select(l => l.Key);
            var commonKeys = oldKeys.Intersect(newKeys).ToList();
            foreach (var key in commonKeys)
            {
                var oldLine = oldPatchLines.Single(l => l.Key == key);
                var newLine = newPatchLines.Single(l => l.Key == key);
                var lineDiff = CompareValues(oldLine.Values, newLine.Values, oldLine.Key!);
                if (lineDiff != null)
                {
                    lineDiffs.Add(lineDiff);
                }
            }
            return lineDiffs;
        }

        private static LineDiff? CompareValues(Dictionary<string, string?> oldValues, Dictionary<string, string?> newValues, MasterKey key)
        {
            var lineDiff = new LineDiff(key);
            // dictionary keys are the same in both, iterating using old one
            foreach (var fieldName in oldValues.Keys)
            {
                var newValue = newValues[fieldName];
                var oldValue = oldValues[fieldName];

                // actual change is when new value is not null, and it's different from the old one
                if (string.Equals(newValue,null) || string.Equals(newValue, oldValue, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                lineDiff.ValueDiffs.Add(fieldName, new ValueDiff(oldValue, newValue));
            }

            return lineDiff.ValueDiffs.Any() ? lineDiff : null; // null would indicate "no changes"
        }
    }
}
