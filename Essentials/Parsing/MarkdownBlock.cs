namespace n_ate.Essentials.Parsing
{
    public class MarkdownBlock
    {
        public MarkdownHeader Header { get; internal set; } = new();
        public string Content { get; internal set; } = string.Empty;
        public int IndexInSource { get; internal set; }
        public MarkdownBlock[] ChildBlocks { get; internal set; } = [];

        public override string ToString() => $"H{Header.Level}: {Header.Name}, Content: {Content}";
    }
}