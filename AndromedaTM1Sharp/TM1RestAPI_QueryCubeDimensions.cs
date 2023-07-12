
namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task<string> QueryCubeDimensions(TM1SharpConfig tm1, string cubeName)
        {
            var client = TM1SharpConfig.GetTM1RestClient(tm1);

            var response = await client.GetAsync(tm1.ServerHTTPSAddress + @"/api/v1/Cubes('" + cubeName + "')?$select=Name&$expand=Dimensions");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
