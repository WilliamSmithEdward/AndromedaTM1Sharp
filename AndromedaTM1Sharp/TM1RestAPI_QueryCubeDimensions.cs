
namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously queries the dimensions of a cube using the specified TM1SharpConfig and cube name.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <returns>A string representing the queried cube dimensions.</returns>
        public async static Task<string> QueryCubeDimensionsAsync(TM1SharpConfig tm1, string cubeName)
        {
            var client = tm1.GetTM1RestClient();

            var response = await client.GetAsync(tm1.ServerAddress + @"/api/v1/Cubes('" + cubeName + "')?$select=Name&$expand=Dimensions");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
