using ScrapingGoogle.Models;

namespace ScrapingGoogle.Services
{
    public class SEORankingService
    {
        public SEORankingService() { }

        public string GetRankingsOutput(List<GoogleSearchResult> results, string urlToSeek)
        {
            var positions = results
                .Where(r => r.Url.Contains(urlToSeek, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Position.ToString())
                .ToList();

            return positions.Count != 0 ? string.Join(", ", positions) : "0";
        }
    }
}
