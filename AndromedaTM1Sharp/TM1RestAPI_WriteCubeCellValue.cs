using System.Net.Http.Headers;
using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously writes cell values in a cube using the specified TM1SharpConfig, cube name, and list of cell references.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <param name="cellReferenceList">A list of CellReference objects specifying cell locations and values.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async static Task WriteCubeCellValueAsync(TM1SharpConfig tm1, string cubeName, List<CellReference> cellReferenceList)
        {
            var client = tm1.GetTM1RestClient();

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
