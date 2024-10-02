using System.Text;

#pragma warning disable IDE0290

namespace Patch
{
    public class DiffSummary
    {
        public string KeyName { get; set; }
        public List<string> RemovedFields { get; set; } = [];
        public string OldFile { get; }
        public string NewFile { get; }

        public List<PatchLine> NewLines { get; set; } = [];
        public List<PatchLine> RemovedLines { get; set; } = [];
        public List<LineDiff> ChangedLines { get; set; } = [];

        public DiffSummary(string keyName, string oldFile, string newFile)
        {
            KeyName = keyName;
            OldFile = oldFile;
            NewFile = newFile;
        }
        
    }
}
