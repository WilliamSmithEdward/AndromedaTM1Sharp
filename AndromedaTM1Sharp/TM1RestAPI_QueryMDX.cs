using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously executes an MDX query using the specified TM1SharpConfig and MDX statement.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="mdxStatement">The MDX query statement.</param>
        /// <returns>A string representing the result of the MDX query.</returns>
        public async static Task<string> QueryMDXAsync(TM1SharpConfig tm1, string mdxStatement)
        {
            var client = tm1.GetTM1RestClient();

            var jsonBody = new StringBuilder();

            jsonBody.Append("{\"MDX\":\"" + mdxStatement + "\"}");

            var jsonPayload = new StringContent(jsonBody.ToString(), new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(tm1.ServerAddress + @"/api/v1/ExecuteMDX?$expand=Axes($expand=Hierarchies($select=Name),Tuples($expand=Members($select=Name))),Cells($select=Ordinal,Value)", jsonPayload);

            var content = await response.Content.ReadAsStringAsync();

            var cellsetId = ParseCellsetId(content);

            DeleteCellset(tm1, cellsetId);

            return content;
        }
    }
}
