using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScrapingGoogle.Services;

public class IndexModel : PageModel
{
    private readonly GoogleSearchService _searchService;
    private readonly SEORankingService _rankingService;

    public IndexModel(GoogleSearchService searchService, SEORankingService rankingService)
    {
        _searchService = searchService;
        _rankingService = rankingService;
    }

    [BindProperty]
    public string Keywords { get; set; }

    [BindProperty]
    public string Url { get; set; }

    public string Results { get; set; }
    public string ErrorMessage { get; set; }

    public async Task OnPostAsync()
    {
        var options = new GoogleSearchOptions
        {
            NumResults = 100,
            Lang = "en",
            Timeout = 5000
        };

        var results = await _searchService.GetSearchResultsAsync(Keywords, options);
        Results = _rankingService.GetRankingsOutput(results, Url);
    }
}