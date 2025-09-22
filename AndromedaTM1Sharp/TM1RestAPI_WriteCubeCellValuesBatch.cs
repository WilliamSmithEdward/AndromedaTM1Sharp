using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously writes cell values in batches to a cube using the specified TM1SharpConfig, cube name, and list of cell references.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <param name="cellReferenceList">A list of CellReference objects specifying cell locations and values.</param>
        /// <param name="chunkSize">The number of cell updates to send in each batch (default is 5000).</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async static Task WriteCubeCellValuesBatchAsync(TM1SharpConfig tm1, string cubeName, List<CellReference> cellReferenceList, int chunkSize = 5000)
        {
            if (cellReferenceList == null || cellReferenceList.Count == 0) return;

            var client = tm1.GetTM1RestClient();
            int index = 0;

            while (index < cellReferenceList.Count)
            {
                int take = Math.Min(chunkSize, cellReferenceList.Count - index);

                var jsonBody = new StringBuilder();
                jsonBody.Append(@"{""Updates"":[");

                for (int i = 0; i < take; i++)
                {
                    var cellReference = cellReferenceList[index + i];

                    jsonBody.Append(@"{""Tuple@odata.bind"": [");

                    for (int e = 0; e < cellReference.Elements.Count; e++)
                    {
                        var y = cellReference.Elements[e];

                        jsonBody.Append(@"""Dimensions('")
                               .Append(y.Dimension)
                               .Append(@"')/Hierarchies('")
                               .Append(y.Hierarchy)
                               .Append(@"')/Elements('")
                               .Append(y.Element)
                               .Append(@"')"",");
                    }

                    if (jsonBody[^1] == ',')
                        jsonBody.Length--;

                    jsonBody.Append(@"],");

                    var v = cellReference.Value?.ToString() ?? string.Empty;

                    if (decimal.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
                    {
                        jsonBody.Append(@"""Value"":").Append(num.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }

                    else
                    {
                        jsonBody.Append(@"""Value"":""")
                                .Append(v.Replace(@"""", @"\"""))
                                .Append('"');
                    }

                    jsonBody.Append("},");
                }

                if (jsonBody[^1] == ',')
                    jsonBody.Length--;

                jsonBody.Append("]}");

                using var content = new StringContent(jsonBody.ToString(), Encoding.UTF8, "application/json");

                var url = tm1.ServerAddress + "/api/v1/Cubes('" + cubeName + "')/tm1.UpdateCells";

                using var response = await client.PostAsync(url, content);

                response.EnsureSuccessStatusCode();

                index += take;
            }
        }
    }
}
