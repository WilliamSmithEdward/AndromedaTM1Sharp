using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task<string> QueryMDX(TM1SharpConfig tm1, string mdxStatement)
        {
            var client = tm1.GetTM1RestClient();

            var jsonBody = new StringBuilder();

            jsonBody.Append("{\"MDX\":\"" + mdxStatement + "\"}");

            var jsonPayload = new StringContent(jsonBody.ToString(), new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(tm1.ServerHTTPSAddress + @"/api/v1/ExecuteMDX?$expand=Axes($expand=Hierarchies($select=Name),Tuples($expand=Members($select=Name))),Cells($select=Ordinal,Value)", jsonPayload);

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
