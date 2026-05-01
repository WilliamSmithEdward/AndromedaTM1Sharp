using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Parses JSON data from a dimension members query into a DimensionListModel object.
    /// </summary>
    public class DimensionListJSONParser
    {
        /// <summary>
        /// Parses the JSON data into a DimensionListModel object.
        /// </summary>
        /// <param name="json">The JSON data to parse.</param>
        /// <returns>A DimensionListModel object representing the parsed data, or null if parsing fails.</returns>
        public static DimensionListModel? ToDimensionListModel(string json)
        {
            var model = JsonSerializer.Deserialize<DimensionListModel>(json);

            return model;
        }

        /// <summary>
        /// Represents a dimension members query response model.
        /// </summary>
        public class DimensionListModel
        {
            /// <summary>
            /// Represents metadata for the response JSON object.
            /// </summary>
            [JsonPropertyName("@odata.context")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Represents the list of dimension elements.
            /// </summary>
            [JsonPropertyName("value")]
            public List<DimensionListElement>? Value { get; set; }
        }

        /// <summary>
        /// Represents a dimension element entry.
        /// </summary>
        public class DimensionListElement
        {
            /// <summary>
            /// Represents metadata for the element JSON object.
            /// </summary>
            [JsonPropertyName("@odata.etag")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Gets or sets the element etag.
            /// </summary>
            [JsonIgnore]
            public string? ETag
            {
                get => MetaData;
                set => MetaData = value;
            }

            /// <summary>
            /// Gets or sets the element name.
            /// </summary>
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the element type.
            /// </summary>
            [JsonPropertyName("Type")]
            public string? Type { get; set; }

            /// <summary>
            /// Gets or sets element attributes keyed by attribute name.
            /// </summary>
            [JsonPropertyName("Attributes")]
            public Dictionary<string, object?>? Attributes { get; set; }
        }
    }
}
