namespace n_ate.Essentials.Parsing
{
    public class MarkdownHeader
    {
        public bool HasId { get; internal set; }
        public int? Id { get; set; }
        public string IdPrefix { get; set; } = string.Empty;
        public int Level { get; internal set; }
        public string Name { get; set; } = string.Empty;
        public string Raw { get; internal set; } = string.Empty;
        public string RawId { get; set; } = string.Empty;
    }
}