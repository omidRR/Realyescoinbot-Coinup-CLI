using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Web;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static int successCount = 0;
    private static int failureCount = 0;
    private static int parallelRequests = 50;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Please enter the URL:");
        string url = Console.ReadLine();
        var queryParams = ExtractQueryParams(url);

        while (true)
        {
            var tasks = new Task[parallelRequests];
            for (int i = 0; i < parallelRequests; i++)
            {
                tasks[i] = SendRequest(queryParams);
            }
            await Task.WhenAll(tasks);
            DisplayStatus();
        }
    }

    private static async Task SendRequest(NameValueCollection queryParams)
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

            string launchParams = queryParams["tgWebAppData"];
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

    private static NameValueCollection ExtractQueryParams(string url)
    {
        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Fragment.Substring(1));
        return query;
    }
}
