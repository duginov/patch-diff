using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch
{
    /// <summary>
    /// Represents differences detected while comparing patch lines with the same MasterKey
    /// </summary>
    public record LineDiff
    {
        public MasterKey? Key { get; set; }
        public Dictionary<string, ValueDiff> ValueDiffs { get; set; } = [];

        public LineDiff(MasterKey key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return $"Key:({Key})|Values:{string.Join("|", ValueDiffs.Select(x => $"{x.Key}:{x.Value.OldValue} -> {x.Value.NewValue}"))}";
        }
    }
}
