namespace ScrapingGoogle.Models
{
    public class GoogleSearchResult
    {
        public int Position { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
    }
}
