using ScrapingGoogle.Exceptions;
using ScrapingGoogle.Models;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace ScrapingGoogle.Services
{
    public class GoogleSearchOptions
    {
        public int NumResults { get; set; } = 10;
        public string Lang { get; set; } = "en";
        public int Timeout { get; set; } = 5000;
        public int Start { get; set; }
        public bool Unique { get; set; }
    }

    public class GoogleSearchService(HttpClient? httpClient = null)
    {
        private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
        private readonly Random _random = new();

        private string GetRandomUserAgent()
        {
            var lynxVersion = $"Lynx/{2 + _random.Next(2)}.{8 + _random.Next(2)}.{_random.Next(3)}";
            var libwwwVersion = $"libwww-FM/{2 + _random.Next(2)}.{13 + _random.Next(3)}";
            var sslMmVersion = $"SSL-MM/{1 + _random.Next(1)}.{3 + _random.Next(3)}";
            var opensslVersion = $"OpenSSL/{1 + _random.Next(3)}.{_random.Next(5)}.{_random.Next(10)}";
            return $"{lynxVersion} {libwwwVersion} {sslMmVersion} {opensslVersion}";
        }

        private static Dictionary<string, string> GetQueryParams(string term, GoogleSearchOptions options)
        {
            return new Dictionary<string, string>
            {
                ["q"] = term,
                ["num"] = options.NumResults.ToString(),
                ["hl"] = options.Lang,
                ["start"] = options.Start.ToString(),
            };
        }

        public async Task<List<GoogleSearchResult>> GetSearchResultsAsync(string term, GoogleSearchOptions? options = null)
        {
            try
            {
                options ??= new GoogleSearchOptions();

                var queryParams = GetQueryParams(term, options);

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
                var url = $"https://www.google.com/search?{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", GetRandomUserAgent());
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Cookie", "CONSENT=PENDING+987; SOCS=CAESHAgBEhIaAB");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                return ParseResults(html);
            }
            catch (HttpRequestException ex)
            {
                throw new GoogleSearchException("Google request failed", ex);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new GoogleSearchException("Result parsing failed", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<GoogleSearchResult> ParseResults(string html)
        {
            var results = new List<GoogleSearchResult>();

            // Regex pattern to match html's code blocks
            string pattern = @"<div class=""ezO2md"">.*?<a class=""fuLhoc ZWRArf"" href=""(?<url>/url\?q=[^""]+)[^>]+><span class=""CVA68e qXLe6d fuLhoc ZWRArf"">(?<title>[^<]+)</span>.*?<span class=""fYyStc"">(?<display_url>[^<]+)</span>.*?<span class=""fYyStc"">(?<description>(?:(?!</span>).)*)</span>";

            RegexOptions options = RegexOptions.Singleline;
            MatchCollection matches = Regex.Matches(html, pattern, options);

            int numberIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Groups.Count < 5) continue;

                var rawUrl = match.Groups[1].Value;
                var url = HttpUtility.UrlDecode(
                    rawUrl.StartsWith("/url?q=") ?
                    rawUrl.Split('&')[0].Replace("/url?q=", "") :
                    rawUrl
                );

                if (string.IsNullOrEmpty(url)) continue;
                if (!url.StartsWith("http")) continue;

                results.Add(new GoogleSearchResult
                {
                    Position = numberIndex + 1,
                    Url = url,
                    Title = WebUtility.HtmlDecode(match.Groups[3].Value.Trim()),
                    Description = WebUtility.HtmlDecode(match.Groups[4].Value.Trim())
                });

                numberIndex++;
            }

            return results;
        }
    }
}