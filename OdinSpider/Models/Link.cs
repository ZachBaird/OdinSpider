namespace OdinSpider.Models
{
    public sealed class Link
    {
        public string LinkText { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public bool WasParsed { get; set; } = false;
    }
}
