using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        private static string ParseCellsetId(string content)
        {
            JsonDocument document = JsonDocument.Parse(content);

            return document.RootElement.GetProperty("ID").GetString() ?? "";
        }

        private static async Task DeleteCellsetAsync(TM1SharpConfig tm1, string cellsetId)
        {
            await tm1.GetTM1RestClient().DeleteAsync(tm1.ServerAddress + $"/api/v1/Cellsets('{cellsetId}')");
        }
    }
}
