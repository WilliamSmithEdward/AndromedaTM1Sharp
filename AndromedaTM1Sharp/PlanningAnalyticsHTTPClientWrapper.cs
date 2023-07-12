using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace AndromedaTM1Sharp
{
    internal class PlanningAnalyticsHTTPClientWrapper
    {
        public HttpClient Client { get; private set; } = new HttpClient();

        public static async Task<PlanningAnalyticsHTTPClientWrapper> Initialize(TM1SharpConfig tm1)
        {
            PlanningAnalyticsHTTPClientWrapper wrapper = new PlanningAnalyticsHTTPClientWrapper();

            EncodingProvider provider = new CustomUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);

            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.SslProtocols = SslProtocols.Tls12;

            HttpClient client = new HttpClient(clientHandler);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var payload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mode", "basic"),
                new KeyValuePair<string, string>("username", tm1.UserName),
                new KeyValuePair<string, string>("password", tm1.Password)
            });

            //Login to TM1 environment and initialize cookies
            await client.PostAsync(tm1.ServerHTTPSAddress + "/login/tm1/tm1dev/form/", payload);

            client.DefaultRequestHeaders.Accept.Clear();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            wrapper.Client = client;

            return wrapper;
        }
    }
}
