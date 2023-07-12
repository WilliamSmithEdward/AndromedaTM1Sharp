using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndromedaTM1Sharp
{
    public class CellsetJSONParser
    {
        public static CellSetModel? ParseIntoObject(string json)
        {
            var model = JsonSerializer.Deserialize<CellSetModel>(json);

            return model;
        }

        public class CellSetModel
        {
            [JsonPropertyName("@odata.context")]
            public string? MetaData { get; set; }

            [JsonPropertyName("ID")]
            public string? Id { get; set; }

            [JsonPropertyName("Axes")]
            public List<Axes>? Axes { get; set; }

            [JsonPropertyName("Cells")]
            public List<Cells>? Cells { get; set; }

            public DataTable ToDataTable()
            {
                var dt = new DataTable();

                dt.Columns.Add("rowIndex", typeof(long));

                Axes?[1]?.Hierarchies?.ForEach(x =>
                {
                    dt.Columns.Add(x.Name);
                });

                Axes?[0]?.Tuples?.ForEach(x =>
                {
                    dt.Columns.Add(x?.Members?[0].Name);
                });

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

                long hierarchyColumns = Axes[1].Hierarchies.Count;

                Parallel.ForEach(Axes[1].Tuples, x =>
                {
                    lock (dt)
                    {
                        long rowPosisiton = x.Ordinal;

                        var row = dt.NewRow();

                        row[0] = rowPosisiton;

                        for (int j = 0; j < hierarchyColumns; j++)
                        {
                            row[j + 1] = x.Members[j].Name;
                        }

                        dt.Rows.Add(row);
                    }
                });

                dt = dt.AsEnumerable().OrderBy(x => x.Field<long>("rowIndex")).CopyToDataTable();

                dt.Columns.Remove("rowIndex");

                long totalColumns = dt.Columns.Count;
                long cellColumns = dt.Columns.Count - Axes[1].Hierarchies.Count;

                Parallel.ForEach(Cells, x =>
                {
                    long rowPosisiton = x.Ordinal / cellColumns;

                    long columnPosition = (x.Ordinal % cellColumns) + hierarchyColumns;

                    lock (dt) dt.Rows[(int)rowPosisiton][(int)columnPosition] = x.Value;
                });

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

                return dt;
            }
        }

        public class Axes
        {
            [JsonPropertyName("Ordinal")]
            public long Ordinal { get; set; }

            [JsonPropertyName("Cardinality")]
            public long Cardinality { get; set; }

            [JsonPropertyName("Hierarchies")]
            public List<Hierarchies>? Hierarchies { get; set; }

            [JsonPropertyName("Tuples")]
            public List<Tuples>? Tuples { get; set; }
        }

        public class Hierarchies
        {
            [JsonPropertyName("@odata.etag")]
            public string? MetaData { get; set; }

            [JsonPropertyName("Name")]
            public string? Name { get; set; }
        }

        public class Tuples
        {
            [JsonPropertyName("Ordinal")]
            public long Ordinal { get; set; }

            [JsonPropertyName("Members")]
            public List<Members>? Members {get;set;}
        }

        public class Members
        {
            [JsonPropertyName("Name")]
            public string? Name { get; set; }
        }

        public class Cells
        {
            [JsonPropertyName("Ordinal")]
            public long Ordinal { get; set; }

            [JsonPropertyName("Value"), JsonConverter(typeof(ObjectPrimitiveConverter))]
            public object? Value { get; set; }
        }

        private class ObjectPrimitiveConverter : JsonConverter<object>
        {
            //https://stackoverflow.com/questions/73695510/deserializing-a-data-member-that-could-be-int-or-string-c-sharp

            public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                reader.TokenType switch
                {
                    JsonTokenType.String => reader.GetString(),
                    JsonTokenType.Number when reader.TryGetInt32(out var i) => i,
                    JsonTokenType.Number when reader.TryGetInt64(out var l) => l,
                    JsonTokenType.Number when reader.TryGetDouble(out var d) => d,
                    JsonTokenType.True => true,
                    JsonTokenType.False => false,
                    _ => throw new JsonException(),
                };
            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) =>
                JsonSerializer.Serialize(writer, value, value.GetType());
        }
    }
}
