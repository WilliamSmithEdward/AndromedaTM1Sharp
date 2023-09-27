using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously runs a TM1 process using the specified TM1SharpConfig, process name, and parameters (optional).
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="processName">The name of the TM1 process.</param>
        /// <param name="parameters">A dictionary of parameters for the TM1 process (optional).</param>
        /// <returns>A string representing the result of running the TM1 process.</returns>
        public async static Task<string> RunProcessAsync(TM1SharpConfig tm1, string processName, Dictionary<string, string>? parameters = null)
        {
            var client = tm1.GetTM1RestClient();

            var jsonBody = new StringBuilder();

            if (parameters != null)
            {
                jsonBody.Append("{\"Parameters\":[");

                parameters.AsEnumerable().ToList().ForEach(x =>
                {
                    jsonBody.Append("{\"Name\":\"" + x.Key + "\", \"Value\":\"" + x.Value + "\"}");
                });

                jsonBody.Append("]}");
            }

            var jsonPayload = new StringContent(jsonBody.ToString(), new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(tm1.ServerHTTPSAddress + @"/api/v1/Processes('" + processName + "')/tm1.ExecuteWithReturn", jsonPayload);

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
