using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task<string> QueryView(TM1SharpConfig tm1, string cubeName, string viewName)
        {
            var client = TM1SharpConfig.GetTM1RestClient(tm1);
            
            var response = await client.PostAsync(tm1.ServerHTTPSAddress + "/api/v1/Cubes('" + cubeName + "')/Views('" + viewName + "')/tm1.Execute?$expand=Axes($expand=Hierarchies($select=Name),Tuples($expand=Members($select=Name))),Cells($select=Ordinal,Value)", new StringContent("", Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
