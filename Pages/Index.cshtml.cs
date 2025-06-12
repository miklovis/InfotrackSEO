using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScrapingGoogle.Services;

namespace ScrapingGoogle.Pages
{
    public class IndexModel(GoogleSearchService searchService, SEORankingService rankingService) : PageModel
    {
        private readonly GoogleSearchService _searchService = searchService;
        private readonly SEORankingService _rankingService = rankingService;

        [BindProperty]
        public required string Keywords { get; set; }

        [BindProperty(Name = "TargetUrl")]
        public required string TargetUrl { get; set; }

        public required string Results { get; set; }

        public async Task OnPostAsync()
        {
            var options = new GoogleSearchOptions
            {
                NumResults = 100,
                Lang = "en",
                Timeout = 5000
            };

            var results = await _searchService.GetSearchResultsAsync(Keywords, options);
            Results = _rankingService.GetRankingsOutput(results, TargetUrl);
        }
    }
}