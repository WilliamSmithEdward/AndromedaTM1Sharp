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

            // Split into chunks
            var chunks = cellReferenceList
                .Select((cell, i) => new { cell, i })
                .GroupBy(x => x.i / chunkSize, x => x.cell);

            foreach (var chunk in chunks)
            {
                var updates = new List<object>();

                foreach (var cellReference in chunk)
                {
                    var tuplePaths = new List<string>();

                    foreach (var element in cellReference.Elements)
                    {
                        tuplePaths.Add(
                            $"Dimensions('{element.Dimension}')/Hierarchies('{element.Hierarchy}')/Elements('{element.Element}')"
                        );
                    }

                    // use dictionary so we can name property exactly as "Tuple@odata.bind"
                    var update = new Dictionary<string, object>
                    {
                        ["Tuple@odata.bind"] = tuplePaths,
                        ["Value"] = cellReference.Value ?? 0
                    };

                    updates.Add(update);
                }

                var body = new Dictionary<string, object>
                {
                    ["Updates"] = updates
                };

                var json = System.Text.Json.JsonSerializer.Serialize(
                    body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null }
                );

                Console.WriteLine(json);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{tm1.ServerAddress}/api/v1/Cubes('{cubeName}')/tm1.UpdateCells";

                using var response = await client.PostAsync(url, content);

                Console.WriteLine(response);

                response.EnsureSuccessStatusCode();
            }
        }

    }
}
