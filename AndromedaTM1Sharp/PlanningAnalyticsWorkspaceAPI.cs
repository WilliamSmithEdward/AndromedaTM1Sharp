using System.Net.Http.Headers;
using System.Text.Json;

namespace AndromedaTM1Sharp
{
    public class PlanningAnalyticsWorkspaceAPI
    {
        public async static Task<string> QueryObjectList(TM1SharpConfig tm1)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.Initialize(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/ServerFolders?viewmode=architect&displayControlObjectsOnly=true&displayChores=true");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async static Task<string> QueryCubeList(TM1SharpConfig tm1)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.Initialize(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes?viewmode=architect");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async static Task<string> QueryCubeDimensions(TM1SharpConfig tm1, string cubeName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.Initialize(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes('" + cubeName + "')/Dimensions");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async static Task<string> QueryCubeViews(TM1SharpConfig tm1, string cubeName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.Initialize(tm1);

            var response = await clientWrapper.Client.GetAsync(tm1.ServerHTTPSAddress + "/prism/harmony/tm1serverexplorer/api/v1/Servers('" + tm1.Environment + "')/Cubes('" + cubeName + "')/Views");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async static Task<string> QueryViewCellSet(TM1SharpConfig tm1, string cubeName, string viewName)
        {
            var clientWrapper = await PlanningAnalyticsHTTPClientWrapper.Initialize(tm1);

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
