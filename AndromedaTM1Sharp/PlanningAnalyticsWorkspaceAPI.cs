using System.Net.Http.Headers;
using System.Text.Json;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Provides methods to interact with the Planning Analytics Workspace API.
    /// </summary>
    public class PlanningAnalyticsWorkspaceAPI
    {
        /// <summary>
        /// Asynchronously queries the object list using the specified TM1SharpConfig.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <returns>A string representing the queried object list.</returns>
        public async static Task<string> QueryObjectListAsync(TM1SharpConfig tm1)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.InitializeAsync(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/ServerFolders?viewmode=architect&displayControlObjectsOnly=true&displayChores=true");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Asynchronously queries the cube list using the specified TM1SharpConfig.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <returns>A string representing the queried cube list.</returns>
        public async static Task<string> QueryCubeListAsync(TM1SharpConfig tm1)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.InitializeAsync(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes?viewmode=architect");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Asynchronously queries the dimensions of a cube using the specified TM1SharpConfig and cube name.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <returns>A string representing the queried cube dimensions.</returns>
        public async static Task<string> QueryCubeDimensionsAsync(TM1SharpConfig tm1, string cubeName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.InitializeAsync(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes('" + cubeName + "')/Dimensions");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Asynchronously queries the views of a cube using the specified TM1SharpConfig and cube name.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <returns>A string representing the queried cube views.</returns>
        public async static Task<string> QueryCubeViewsAsync(TM1SharpConfig tm1, string cubeName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.InitializeAsync(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes('" + cubeName + "')/Views");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Asynchronously queries the cell set of a view using the specified TM1SharpConfig, cube name, and view name.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>A string representing the queried cell set.</returns>
        public async static Task<string> QueryViewCellSet(TM1SharpConfig tm1, string cubeName, string viewName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.InitializeAsync(tm1);

            var jsonBodyObject = new
            {
                Server = tm1.Environment,
                Cube = cubeName,
                Private = false,
                RangeStrategy = "RELOCATION",
                View = viewName
            };

            var jsonBody = JsonSerializer.Serialize<object>(jsonBodyObject, new JsonSerializerOptions { WriteIndented = true, });

            var jsonPayload = new StringContent(jsonBody, new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await clientWrapper.Client.PostAsync(tm1.ServerHTTPSAddress + "/prism/harmony/gridservice/api/v1/Create", jsonPayload);

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
