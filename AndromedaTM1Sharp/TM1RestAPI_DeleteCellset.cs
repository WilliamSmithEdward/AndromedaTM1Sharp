using System.Text.Json;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        private static string ParseCellsetId(string content)
        {
            JsonDocument document = JsonDocument.Parse(content);

            if (!document.RootElement.TryGetProperty("ID", out JsonElement idElement))
            {
                return "";
            }

            return idElement.GetString() ?? "";
        }

        private static void DeleteCellset(TM1SharpConfig tm1, string cellsetId)
        {
            if (string.IsNullOrEmpty(cellsetId))
            {
                return;
            }

            Task.Run(() => tm1.GetTM1RestClient().DeleteAsync(tm1.ServerAddress + $"/api/v1/Cellsets('{cellsetId}')"));
        }
    }
}
