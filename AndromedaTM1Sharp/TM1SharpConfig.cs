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
        public bool IgnoreSSLCertError { get; private set; }

        public TM1SharpConfig(string tm1ServerURL, string userName, string password, string environment, bool ignoreSSLCertError = false)
        {
            ServerHTTPSAddress = tm1ServerURL.TrimEnd('/');
            UserName = userName;
            Password = password;
            Environment = environment;
            IgnoreSSLCertError = ignoreSSLCertError;
        }

        internal HttpClient GetTM1RestClient()
        {
            EncodingProvider provider = new CustomUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);

            HttpClientHandler clientHandler = new HttpClientHandler();
            if (IgnoreSSLCertError) clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.SslProtocols = SslProtocols.Tls12;

            HttpClient client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password)));

            return client;
        }
    }
}
