namespace Patch
{
    // PatchLine is used for two purposes:
    // 1. Keep deserialized info from patch file
    // 2. Keep reportable info about changes due to addition and removal of patch lines
    public record PatchLine
    {
        public MasterKey? Key { get; set; }
        public Dictionary<string, string?> Values { get; set; } = [];

        public override string ToString()
        {
            return $"\tKey:({Key})|Values:{string.Join("|", Values.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}";
        }
    }
}
