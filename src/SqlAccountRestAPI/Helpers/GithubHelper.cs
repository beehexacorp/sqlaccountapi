using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SqlAccountRestAPI.Helpers
{
    public class GithubHelper
    {
        static public async Task<IDictionary<string, object>> GetLatestReleaseInfo()
        {
            string url = $"https://api.github.com/repos/{ApplicationConstants.GITHUB_SQL_ACCOUNT_REST_API_RELEASE_URL}latest";
            using HttpClient client = new HttpClient();

            // Add a User-Agent header required by GitHub API
            client.DefaultRequestHeaders.Add("User-Agent", "SqlAccountRestAPIMonitor");

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response
                string json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                // Extract "tag_name" from response
                return release ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest version: {ex.Message}");
                return new Dictionary<string, object>
                    {
                        { "Error", ex.Message }
                    };
            }
        }
        static public async Task<string> GetDownloadUrl()
        {
            try
            {
                // Call to GitHub API to fetch latest release information
                var releaseInfo = await GetLatestReleaseInfo();

                // Check if the "assets" field is present and is a List of Dictionaries
                if (releaseInfo.ContainsKey("assets") && ((JsonElement)releaseInfo["assets"]).ValueKind == JsonValueKind.Array)
                {
                    // Iterate through each asset object in the array
                    foreach (var assetObj in ((JsonElement)releaseInfo["assets"]).EnumerateArray())
                    {
                        // Check if "name" and "browser_download_url" are present in the asset object
                        if (assetObj.TryGetProperty("name", out var nameProp) &&
                            assetObj.TryGetProperty("browser_download_url", out var urlProp))
                        {
                            var assetName = nameProp.GetString();
                            var downloadUrl = urlProp.GetString();

                            // If we find an asset that ends with "win-x64.zip", return its download URL
                            if (assetName != null && assetName.EndsWith("win-x64.zip"))
                            {
                                return downloadUrl!;
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching release: {ex.Message}");
                return ex.Message;
            }
        }
    }
}