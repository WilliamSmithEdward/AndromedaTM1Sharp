using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task<string> RunProcess(TM1SharpConfig tm1, string processName, Dictionary<string, string>? parameters = null)
        {
            var client = TM1SharpConfig.GetTM1RestClient(tm1);

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
