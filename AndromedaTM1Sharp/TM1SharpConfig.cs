using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Represents the configuration settings for TM1Sharp.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TM1SharpConfig"/> class with the specified parameters.
    /// </remarks>
    /// <param name="tm1ServerURL">The TM1 server URL.</param>
    /// <param name="userName">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="environment">The environment information.</param>
    /// <param name="ignoreSSLCertError">A value indicating whether to ignore SSL certificate errors (default is false).</param>
    public class TM1SharpConfig(string tm1ServerURL, string userName, string password, string environment, bool ignoreSSLCertError = false)
    {
        /// <summary>
        /// Gets the TM1 server HTTPS address.
        /// </summary>
        public string ServerAddress { get; private set; } = tm1ServerURL.TrimEnd('/');

        /// <summary>
        /// Gets the username for authentication.
        /// </summary>
        public string UserName { get; private set; } = userName;

        /// <summary>
        /// Gets the password for authentication.
        /// </summary>
        public string Password { get; private set; } = password;

        /// <summary>
        /// Gets the environment information.
        /// </summary>
        public string Environment { get; private set; } = environment;

        /// <summary>
        /// Gets a value indicating whether to ignore SSL certificate errors.
        /// </summary>
        public bool IgnoreSSLCertError { get; private set; } = ignoreSSLCertError;

        internal HttpClient GetTM1RestClient(string acceptType = "application/json")
        {
            var provider = new CustomUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);

            var clientHandler = new HttpClientHandler();

            if (IgnoreSSLCertError) clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
                clientHandler.SslProtocols = SslProtocols.Tls12;

            var client = new HttpClient(clientHandler);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password)));

            return client;
        }
    }
}
