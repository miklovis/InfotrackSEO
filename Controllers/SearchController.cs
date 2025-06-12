using Microsoft.AspNetCore.Mvc;
using ScrapingGoogle.Services;

namespace ScrapingGoogle.Controllers
{
    public class SearchController : Controller
    {
        private readonly GoogleSearchService _googleSearchService;
        private readonly SEORankingService _seoRankingService;

        public SearchController()
        {
            var httpClient = new HttpClient();
            _googleSearchService = new GoogleSearchService(httpClient);
            _seoRankingService = new SEORankingService();
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term, string urlToSeek)
        {
            try
            {
                var options = new GoogleSearchOptions
                {
                    NumResults = 100,
                    Lang = "en",
                    Timeout = 5000
                };

                var results = await _googleSearchService.GetSearchResultsAsync(term, options);

                var rankingString = _seoRankingService.GetRankingsOutput(results, urlToSeek);
                return Ok(rankingString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Search failed: {ex.Message}");
            }
        }
    }
}