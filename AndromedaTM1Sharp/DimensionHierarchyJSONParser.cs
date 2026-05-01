using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Parses JSON data from a dimension hierarchy rollup query into a DimensionHierarchyModel object.
    /// </summary>
    public class DimensionHierarchyJSONParser
    {
        /// <summary>
        /// Parses the JSON data into a DimensionHierarchyModel object.
        /// </summary>
        /// <param name="json">The JSON data to parse.</param>
        /// <returns>A DimensionHierarchyModel object representing the parsed data, or null if parsing fails.</returns>
        public static DimensionHierarchyModel? ToDimensionHierarchyModel(string json)
        {
            var model = JsonSerializer.Deserialize<DimensionHierarchyModel>(json);

            return model;
        }

        /// <summary>
        /// Represents a dimension hierarchy rollup response model.
        /// </summary>
        public class DimensionHierarchyModel
        {
            /// <summary>
            /// Represents metadata for the response JSON object.
            /// </summary>
            [JsonPropertyName("@odata.context")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Represents the list of hierarchy elements.
            /// </summary>
            [JsonPropertyName("value")]
            public List<HierarchyElement>? Value { get; set; }

            /// <summary>
            /// Flattens hierarchy component relationships into parent-child-weight edges.
            /// </summary>
            /// <returns>A list of hierarchy edges.</returns>
            public List<HierarchyEdge> ToEdges()
            {
                var edges = new List<HierarchyEdge>();

                foreach (var parent in Value ?? Enumerable.Empty<HierarchyElement>())
                {
                    var parentName = parent?.Name;

                    if (string.IsNullOrWhiteSpace(parentName)) continue;

                    foreach (var component in parent?.Components ?? Enumerable.Empty<HierarchyComponent>())
                    {
                        var childName = component?.Name ?? component?.Component?.Name;

                        if (string.IsNullOrWhiteSpace(childName)) continue;

                        edges.Add(new HierarchyEdge()
                        {
                            Parent = parentName,
                            Child = childName,
                            Weight = component?.Weight ?? 0,
                            ParentETag = parent?.ETag,
                            ChildETag = component?.Component?.ETag,
                            EdgeETag = component?.ETag
                        });
                    }
                }

                return edges;
            }
        }

        /// <summary>
        /// Represents an element entry in the hierarchy.
        /// </summary>
        public class HierarchyElement
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
            /// Gets or sets child components of this element.
            /// </summary>
            [JsonPropertyName("Components")]
            public List<HierarchyComponent>? Components { get; set; }
        }

        /// <summary>
        /// Represents a child component relationship.
        /// </summary>
        public class HierarchyComponent
        {
            /// <summary>
            /// Represents metadata for the component/edge JSON object.
            /// </summary>
            [JsonPropertyName("@odata.etag")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Gets or sets the component/edge etag.
            /// </summary>
            [JsonIgnore]
            public string? ETag
            {
                get => MetaData;
                set => MetaData = value;
            }

            /// <summary>
            /// Gets or sets the child element name.
            /// </summary>
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets nested child component element details.
            /// </summary>
            [JsonPropertyName("Component")]
            public HierarchyComponentElement? Component { get; set; }

            /// <summary>
            /// Gets or sets the rollup weight.
            /// </summary>
            [JsonPropertyName("Weight")]
            public double Weight { get; set; }
        }

        /// <summary>
        /// Represents nested child component element details.
        /// </summary>
        public class HierarchyComponentElement
        {
            /// <summary>
            /// Represents metadata for the nested child element JSON object.
            /// </summary>
            [JsonPropertyName("@odata.etag")]
            public string? MetaData { get; set; }

            /// <summary>
            /// Gets or sets the child element etag.
            /// </summary>
            [JsonIgnore]
            public string? ETag
            {
                get => MetaData;
                set => MetaData = value;
            }

            /// <summary>
            /// Gets or sets the child element name.
            /// </summary>
            [JsonPropertyName("Name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Represents a flattened parent-child relationship row.
        /// </summary>
        public class HierarchyEdge
        {
            /// <summary>
            /// Gets or sets the parent element name.
            /// </summary>
            public string? Parent { get; set; }

            /// <summary>
            /// Gets or sets the child element name.
            /// </summary>
            public string? Child { get; set; }

            /// <summary>
            /// Gets or sets the rollup weight.
            /// </summary>
            public double Weight { get; set; }

            /// <summary>
            /// Gets or sets the parent element etag.
            /// </summary>
            public string? ParentETag { get; set; }

            /// <summary>
            /// Gets or sets the child element etag.
            /// </summary>
            public string? ChildETag { get; set; }

            /// <summary>
            /// Gets or sets the edge/component etag.
            /// </summary>
            public string? EdgeETag { get; set; }
        }
    }
}
