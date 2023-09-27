using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Represents the configuration settings for TM1Sharp.
    /// </summary>
    public class TM1SharpConfig
    {
        /// <summary>
        /// Gets the TM1 server HTTPS address.
        /// </summary>
        public string ServerHTTPSAddress { get; private set; }

        /// <summary>
        /// Gets the username for authentication.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the password for authentication.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the environment information.
        /// </summary>
        public string Environment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to ignore SSL certificate errors.
        /// </summary>
        public bool IgnoreSSLCertError { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TM1SharpConfig"/> class with the specified parameters.
        /// </summary>
        /// <param name="tm1ServerURL">The TM1 server URL.</param>
        /// <param name="userName">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <param name="environment">The environment information.</param>
        /// <param name="ignoreSSLCertError">A value indicating whether to ignore SSL certificate errors (default is false).</param>
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
            var provider = new CustomUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);

            var clientHandler = new HttpClientHandler();
            if (IgnoreSSLCertError) clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
            clientHandler.SslProtocols = SslProtocols.Tls12;

            var client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password)));

            return client;
        }
    }
}
