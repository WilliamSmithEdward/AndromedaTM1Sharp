namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously queries the cube list using the specified TM1SharpConfig.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <returns>A string representing the queried cube list.</returns>
        public async static Task<string> QueryCubeListAsync(TM1SharpConfig tm1)
        {
            var client = tm1.GetTM1RestClient();

            var content = await client.GetStringAsync(tm1.ServerAddress + "/api/v1/Cubes");

            return content;
        }
    }
}
