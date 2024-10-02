namespace Patch
{
    /// <summary>
    /// Represents deserialized patch file.
    /// Contains few methods to shape the data to the form convenient for comparison
    /// </summary>
    public class Patch
    {
        public string? KeyName { get; set; }
        public List<string> ValueFields { get; set; } = [];
        public List<PatchLine> Lines { get; set; } = [];
        public string? FileName { get; set; }

        public override string ToString()
        {
            return $"FileName:{Path.GetFileName(FileName)}|KeyName:{KeyName}|ValueFields:{string.Join(",",ValueFields)}|Lines:\r\n{string.Join($"{Environment.NewLine}", Lines)}";
        }

        public void AddFields(List<string> addedFieldNames)
        {
            ValueFields.AddRange(addedFieldNames);
            foreach (var line in Lines)
            {
                foreach (var addedValueField in addedFieldNames)
                {
                    line.Values.Add(addedValueField,null);
                }
            }
        }

        public void RemoveFields(List<string> fieldsToRemove)
        {
            ValueFields.RemoveAll(fieldsToRemove.Contains);
            foreach (var line in Lines)
            {
                foreach (var fieldToRemove in fieldsToRemove)
                {
                    line.Values.Remove(fieldToRemove);
                }
            }
        }

        /// <summary>
        /// More than one line with the same MasterKey may appear in a file
        /// This method "merges" values from those lines top-to-bottom to simplify comparison
        /// </summary>
        public void ConsolidateSameKeyLines()
        {
            // find all keys appearing in more than one line in the patch
            var keysToConsolidate = Lines
                .GroupBy(pl => pl.Key)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var masterKey in keysToConsolidate)
            {
                var resultingLine = new PatchLine { Key = masterKey };
                foreach (var line in Lines.Where(l => l.Key == masterKey))
                {
                    foreach (var field in ValueFields)
                    {
                        // overwrite "older" value only if value in current line is not null
                        if (line.Values[field] != null)
                        {
                            resultingLine.Values[field] = line.Values[field];
                        }
                    }
                }

                // at the end of line consolidation add empty value if it was never set
                foreach (var field in ValueFields)
                {
                    resultingLine.Values.TryAdd(field, null);
                }

                // remove all original lines and add back one consolidated line
                Lines.RemoveAll(pl => pl.Key!.Equals(masterKey));
                Lines.Add(resultingLine);
            }
        }


    }
}
