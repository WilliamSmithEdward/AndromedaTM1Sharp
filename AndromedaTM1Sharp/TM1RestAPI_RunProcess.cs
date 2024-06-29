using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
        /// <remarks>
        /// <para><c>Warning:</c> In testing, when launching long running Turbo Integrator processes,
        /// the TM1 server has in some instances unexpectedly returned with no JSON response.
        /// It is recommended to use RunProcessWithPollingAsync if this is a concern.</para>
        /// </remarks>
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

            var response = await client.PostAsync(tm1.ServerAddress + @"/api/v1/Processes('" + processName + "')/tm1.ExecuteWithReturn", jsonPayload);

            var content = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(content)?.RootElement.GetProperty("ProcessExecuteStatusCode").GetString() ?? "UnknownError";
        }

        /// <summary>
        /// Runs a TM1 process asynchronously with polling for the status until completion.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig object containing configuration for the TM1 server.</param>
        /// <param name="processName">The name of the TM1 process to be executed.</param>
        /// <param name="timeoutSeconds">The number of seconds to wait for the process to complete before throwing a timeout exception.</param>
        /// <param name="parameters">Optional dictionary containing parameters for the process.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the process execution status code.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="JsonException">Thrown when parsing the JSON response fails.</exception>
        /// <exception cref="TimeoutException">Thrown when the request times out.</exception>
        public async static Task<string> RunProcessWithPollingAsync(TM1SharpConfig tm1, string processName, int timeoutSeconds = 60, Dictionary<string, string>? parameters = null)
        {
            var client = tm1.GetTM1RestClient();

            client.DefaultRequestHeaders.Add("Prefer", "respond-async");

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

            var response = await client.PostAsync(tm1.ServerAddress + @"/api/v1/Processes('" + processName + "')/tm1.ExecuteWithReturn", jsonPayload);

            var asyncId = ParseAsyncId(response.Headers.Where(x => x.Key.Equals("Location")).First().Value.First());

            var asyncResult = await PollAsyncProcessForResult(client, tm1.ServerAddress, asyncId, timeoutSeconds);

            return JsonDocument.Parse(asyncResult)?.RootElement.GetProperty("ProcessExecuteStatusCode").GetString() ?? "UnknownError";
        }
    }
}
