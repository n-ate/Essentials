namespace n_ate.Essentials.Parsing
{
    public class MarkdownSection
    {
        public MarkdownHeader Header { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public int IndexInSource { get; internal set; }

        public override string ToString() => $"H{Header.Level}: {Header.Name}, Content: {Content}";
    }
}