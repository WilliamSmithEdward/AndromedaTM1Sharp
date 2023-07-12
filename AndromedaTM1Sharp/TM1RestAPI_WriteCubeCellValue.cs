using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        public async static Task WriteCubeCellValue(TM1SharpConfig tm1, string cubeName, List<CellReference> cellReferenceList)
        {
            var client = TM1SharpConfig.GetTM1RestClient(tm1);

            foreach (var cellReference in cellReferenceList)
            {
                var jsonBody = new StringBuilder();

                jsonBody.Append(@"{""Cells"":[{""Tuple@odata.bind"": [");

                cellReference.Elements.ForEach(y =>
                {
                    jsonBody.Append(
                        @"""Dimensions('" + y.Dimension + @"') / Hierarchies('" + y.Hierarchy + @"') / Elements('" + y.Element + @"')"","
                    );
                });

                jsonBody.Append(@"]}],""Value"":""" + cellReference.Value + @"""}");

                var jsonPayload = new StringContent(jsonBody.ToString(), new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(tm1.ServerHTTPSAddress + "/api/v1/Cubes('" + cubeName + "')/tm1.Update", jsonPayload);

                var content = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
