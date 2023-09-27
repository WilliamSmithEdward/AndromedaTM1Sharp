using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously queries a view using the specified TM1SharpConfig, cube name, and view name.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>A string representing the queried view.</returns>
        public async static Task<string> QueryViewAsync(TM1SharpConfig tm1, string cubeName, string viewName)
        {
            var client = tm1.GetTM1RestClient();
            
            var response = await client.PostAsync(tm1.ServerHTTPSAddress + "/api/v1/Cubes('" + cubeName + "')/Views('" + viewName + "')/tm1.Execute?$expand=Axes($expand=Hierarchies($select=Name),Tuples($expand=Members($select=Name))),Cells($select=Ordinal,Value)", new StringContent("", Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
