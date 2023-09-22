namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task<string> QueryCubeList(TM1SharpConfig tm1)
        {
            var client = tm1.GetTM1RestClient();

            var content = await client.GetStringAsync(tm1.ServerHTTPSAddress + "/api/v1/Cubes");

            return content;
        }
    }
}
