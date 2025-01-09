using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Parses JSON data into a CellSetModel object.
    /// </summary>
    public class CellsetJSONParser
    {
        /// <summary>
        /// Parses the JSON data into a CellSetModel object.
        /// </summary>
        /// <param name="json">The JSON data to parse.</param>
        /// <returns>A CellSetModel object representing the parsed data, or null if parsing fails.</returns>
        public static CellSetModel? ParseIntoObject(string json)
        {
            var model = JsonSerializer.Deserialize<CellSetModel>(json);

            return model;
        }

        /// <summary>
        /// Represents a TM1 cell set model.
        /// </summary>
        public class CellSetModel
        {
            /// <summary>
            /// Represents metadata for the CellSetModel JSON object.
            /// </summary>
            [JsonPropertyName("@odata.context")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Represents an ID for the CellSetModel JSON object.
            /// </summary>
            [JsonPropertyName("ID")]
            public string? Id { get; set; }

            /// <summary>
            /// Represents axes for the CellSetModel JSON object.
            /// </summary>
            [JsonPropertyName("Axes")]
            public List<Axes>? Axes { get; set; }

            /// <summary>
            /// Represents cells for the CellSetModel JSON object.
            /// </summary>
            [JsonPropertyName("Cells")]
            public List<Cells>? Cells { get; set; }

            /// <summary>
            /// Converts the data to a DataTable.
            /// </summary>
            /// <returns>A DataTable representation of the data.</returns>
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

                int? hierarchyColumns = Axes?[1]?.Hierarchies?.Count;

                Parallel.ForEach(Axes?[1]?.Tuples ?? Enumerable.Empty<Tuples>(), x =>
                {
                    lock (dt)
                    {
                        long rowPosisiton = x.Ordinal;

                        var row = dt.NewRow();

                        row[0] = rowPosisiton;

                        for (int j = 0; j < hierarchyColumns; j++)
                        {
                            row[j + 1] = x?.Members?[j].Name;
                        }

                        dt.Rows.Add(row);
                    }
                });

                dt = dt.AsEnumerable().OrderBy(x => x.Field<long>("rowIndex")).CopyToDataTable();

                dt.Columns.Remove("rowIndex");

                int totalColumns = dt.Columns.Count;
                int? cellColumns = dt.Columns.Count - Axes?[1]?.Hierarchies?.Count;

                Parallel.ForEach(Cells ?? Enumerable.Empty<Cells>(), x =>
                {
                    int rowPosisiton = x.Ordinal / cellColumns ?? 0;

                    int columnPosition = (x.Ordinal % cellColumns) + hierarchyColumns ?? 0;

                    lock (dt) dt.Rows[rowPosisiton][columnPosition] = x.Value ?? 0;
                });

                return dt;
            }
        }

        /// <summary>
        /// Represents axes information.
        /// </summary>
        public class Axes
        {
            /// <summary>
            /// Gets or sets the ordinal value.
            /// </summary>
            [JsonPropertyName("Ordinal")]
            public int Ordinal { get; set; }

            /// <summary>
            /// Gets or sets the cardinality value.
            /// </summary>
            [JsonPropertyName("Cardinality")]
            public int Cardinality { get; set; }

            /// <summary>
            /// Gets or sets the list of hierarchies.
            /// </summary>
            [JsonPropertyName("Hierarchies")]
            public List<Hierarchies>? Hierarchies { get; set; }

            /// <summary>
            /// Gets or sets the list of tuples.
            /// </summary>
            [JsonPropertyName("Tuples")]
            public List<Tuples>? Tuples { get; set; }
        }

        /// <summary>
        /// Represents hierarchies information.
        /// </summary>
        public class Hierarchies
        {
            /// <summary>
            /// Gets or sets the metadata value.
            /// </summary>
            [JsonPropertyName("@odata.etag")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Gets or sets the name of the hierarchy.
            /// </summary>
            [JsonPropertyName("Name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Represents tuples information.
        /// </summary>
        public class Tuples
        {
            /// <summary>
            /// Gets or sets the ordinal value.
            /// </summary>
            [JsonPropertyName("Ordinal")]
            public int Ordinal { get; set; }

            /// <summary>
            /// Gets or sets the list of members.
            /// </summary>
            [JsonPropertyName("Members")]
            public List<Members>? Members { get; set; }
        }

        /// <summary>
        /// Represents member information.
        /// </summary>
        public class Members
        {
            /// <summary>
            /// Gets or sets the name of the member.
            /// </summary>
            [JsonPropertyName("Name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Represents cells information.
        /// </summary>
        public class Cells
        {
            /// <summary>
            /// Gets or sets the ordinal value.
            /// </summary>
            [JsonPropertyName("Ordinal")]
            public int Ordinal { get; set; }

            /// <summary>
            /// Gets or sets the value of the cell.
            /// </summary>
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
