namespace Patch
{
    public record ValueDiff
    {
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public ValueDiff(string? oldValue, string? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return $"{OldValue ?? "(none)"} -> {NewValue ?? "(none)"}";
        }
    }
}
