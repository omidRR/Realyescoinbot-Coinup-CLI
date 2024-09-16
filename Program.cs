using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Web;
using System.Text.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static int successCount = 0;
    private static int failureCount = 0;
    private static int parallelRequests = 50;

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Please enter the URL or query string:");
            string input = Console.ReadLine();

            NameValueCollection queryParams;
            Uri uri = null;

            if (input.StartsWith("http://") || input.StartsWith("https://"))
            {
                uri = new Uri(input);
                queryParams = ExtractQueryParamsFromUri(uri);
            }
            else
            {
                queryParams = HttpUtility.ParseQueryString(input);
            }

            var timer = new System.Timers.Timer(10000);
            timer.Elapsed += async (sender, e) => await GetGoldInfo(queryParams, uri);
            timer.Start();

            while (true)
            {
                var tasks = new Task[parallelRequests];
                for (int i = 0; i < parallelRequests; i++)
                {
                    tasks[i] = SendRequest(queryParams, uri);
                }
                await Task.WhenAll(tasks);
                DisplayStatus();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
       
    }

    private static async Task SendRequest(NameValueCollection queryParams, Uri uri)
    {
        try
        {
            var boundary = "reqable-f01e4f71-3006-11ef-8b21-8741560245b7";
            var content = new MultipartFormDataContent(boundary);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            content.Add(new StringContent(timestamp), "viewCompletedAt");
            content.Add(new StringContent("81"), "reference");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://clownfish-app-f7unk.ondigitalocean.app/v2/tasks/claimAdsgramAdReward");
            request.Headers.Add("Reqable-Id", "reqable-id-ea1b4318-bc3d-431e-9ef5-980b7da9e59d");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("accept-language", "en-US");

            string launchParams = queryParams["tgWebAppData"] ?? queryParams.ToString();
            request.Headers.Add("launch-params", launchParams);
            request.Headers.Add("origin", "https://miniapp.yesco.in");
            request.Headers.Add("sec-fetch-site", "cross-site");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("referer", "https://miniapp.yesco.in/");
            request.Headers.Add("priority", "u=4, i");

            if (queryParams["cookie"] != null)
            {
                request.Headers.Add("Cookie", queryParams["cookie"]);
            }

            content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
            request.Content = content;

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Interlocked.Increment(ref successCount);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref failureCount);
        }
    }

    private static void DisplayStatus()
    {
        Console.Write($"\rSuccess: {successCount} | Failure: {failureCount}");
    }

    private static NameValueCollection ExtractQueryParamsFromUri(Uri uri)
    {
        return HttpUtility.ParseQueryString(uri.Fragment.Substring(1));
    }

    private static async Task GetGoldInfo(NameValueCollection queryParams, Uri uri)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://clownfish-app-f7unk.ondigitalocean.app/v2/user/getInfo");
            request.Headers.Add("accept", "*/*");
            request.Headers.Add("accept-language", "en-US,en;q=0.9,fa;q=0.8");
            request.Headers.Add("dnt", "1");

            string launchParams = queryParams["tgWebAppData"] ?? queryParams.ToString();
            request.Headers.Add("launch-params", launchParams);
            request.Headers.Add("origin", "https://miniapp.yesco.in");
            request.Headers.Add("priority", "u=1, i");
            request.Headers.Add("referer", "https://miniapp.yesco.in/");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");

            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            request.Content = content;

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(responseString);
            var goldData = jsonDoc.RootElement.GetProperty("payload").GetProperty("scoreData");
            var oldGold = goldData.GetProperty("oldGold").GetInt32();
            var gold = goldData.GetProperty("gold").GetInt32();

            Console.WriteLine($" | Balance==> {gold}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}
