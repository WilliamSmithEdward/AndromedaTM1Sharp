using System.Text.RegularExpressions;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        private async static Task<string> PollAsyncProcessForResult(HttpClient client, string baseUrl, string asyncId, int timeoutSeconds)
        {
            int counter = 1;

            while (true)
            {        
                var response = await client.GetAsync($"{baseUrl}/api/v1/_async('{asyncId}')");

                int statusCode = (int)response.StatusCode;

                if (statusCode == 200 || statusCode == 201)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                if (counter == timeoutSeconds)
                {
                    throw new TimeoutException("The request timed out.");
                }

                counter++;

                await Task.Delay(1000);
            }
        }

        private static string ParseAsyncId(string asyncId)
        {
            string pattern = @"\('([^']+)'\)";

            Match match = Regex.Match(asyncId, pattern);

            if (match.Success)
            {
                string id = match.Groups[1].Value;
                return id;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
