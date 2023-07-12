using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace AndromedaTM1Sharp
{
    public class TM1SharpConfig
    {
        public string ServerHTTPSAddress { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string Environment { get; private set; }

        public TM1SharpConfig(string tm1ServerURL, string userName, string password, string environment)
        {
            ServerHTTPSAddress = tm1ServerURL.TrimEnd('/');
            UserName = userName;
            Password = password;
            Environment = environment;
        }

        internal static HttpClient GetTM1RestClient(TM1SharpConfig tm1)
        {
            EncodingProvider provider = new CustomUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.SslProtocols = SslProtocols.Tls12;

            HttpClient client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(tm1.UserName + ":" + tm1.Password)));

            return client;
        }
    }
}
