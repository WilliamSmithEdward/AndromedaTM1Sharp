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
        /// Represents the role of an element within the hierarchy.
        /// </summary>
        public enum NodeRole
        {
            /// <summary>A consolidation that is never a child of another element.</summary>
            Root,
            /// <summary>A consolidation that is also a child of another element.</summary>
            Member,
            /// <summary>An element that is only ever a child, never a parent.</summary>
            Leaf,
            /// <summary>An element that has no parent and no children.</summary>
            Orphan
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
            /// All members in the dimension, including those with no parent/child relationships.
            /// Populated during enrichment.
            /// </summary>
            [JsonIgnore]
            public List<AllMember>? AllMembers { get; set; }

            /// <summary>
            /// Flattens hierarchy component relationships into parent-child-weight edges,
            /// including node roles (Root/Member/Leaf/Orphan) and depth levels.
            /// Roots and orphans appear as null-parent edges so every member is represented as Child.
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
                            ParentType = parent?.Type,
                            Child = childName,
                            ChildType = component?.Component?.Type,
                            Weight = component?.Weight ?? 0,
                            ParentETag = parent?.ETag,
                            ChildETag = component?.Component?.ETag,
                            EdgeETag = component?.ETag,
                            ParentAttributes = parent?.Attributes,
                            ChildAttributes = component?.Component?.Attributes
                        });
                    }
                }

                EnrichEdgesWithRolesAndLevels(edges, AllMembers);

                return edges;
            }

            private static void EnrichEdgesWithRolesAndLevels(List<HierarchyEdge> edges, List<AllMember>? allMembers)
            {
                var parentSet = edges.Select(e => e.Parent!).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var childSet  = edges.Select(e => e.Child!).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Build children map for BFS level calculation
                var childrenOf = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var e in edges)
                {
                    if (!childrenOf.ContainsKey(e.Parent!)) childrenOf[e.Parent!] = [];
                    childrenOf[e.Parent!].Add(e.Child!);
                }

                // BFS from roots to assign levels
                var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var roots = parentSet.Except(childSet, StringComparer.OrdinalIgnoreCase).ToList();
                var queue = new Queue<(string Name, int Level)>();

                foreach (var root in roots)
                {
                    levels[root] = 0;
                    queue.Enqueue((root, 0));
                }

                while (queue.Count > 0)
                {
                    var (name, level) = queue.Dequeue();

                    if (!childrenOf.TryGetValue(name, out var children)) continue;

                    foreach (var child in children)
                    {
                        if (!levels.ContainsKey(child))
                        {
                            levels[child] = level + 1;
                            queue.Enqueue((child, level + 1));
                        }
                    }
                }

                foreach (var edge in edges)
                {
                    var parentIsChild = childSet.Contains(edge.Parent!);
                    edge.ParentRole = parentIsChild ? NodeRole.Member : NodeRole.Root;
                    edge.ParentLevel = levels.TryGetValue(edge.Parent!, out var pl) ? pl : 0;

                    var childIsParent = parentSet.Contains(edge.Child!);
                    edge.ChildRole = childIsParent ? NodeRole.Member : NodeRole.Leaf;
                    edge.ChildLevel = levels.TryGetValue(edge.Child!, out var cl) ? cl : (edge.ParentLevel ?? 0) + 1;
                }

                // Emit null-parent edges for roots and orphans
                if (allMembers is null) return;

                var allInEdges = childSet.Union(parentSet, StringComparer.OrdinalIgnoreCase)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var member in allMembers)
                {
                    if (string.IsNullOrWhiteSpace(member.Name)) continue;

                    var isRoot   = parentSet.Contains(member.Name) && !childSet.Contains(member.Name);
                    var isOrphan = !parentSet.Contains(member.Name) && !childSet.Contains(member.Name);

                    if (!isRoot && !isOrphan) continue;

                    var role  = isOrphan ? NodeRole.Orphan : NodeRole.Root;
                    var level = levels.TryGetValue(member.Name, out var l) ? l : 0;

                    edges.Add(new HierarchyEdge
                    {
                        Parent          = null,
                        ParentType      = null,
                        ParentRole      = null,
                        ParentLevel     = null,
                        Child           = member.Name,
                        ChildType       = member.Type,
                        ChildRole       = role,
                        ChildLevel      = level,
                        ChildETag       = member.ETag,
                        ChildAttributes = member.Attributes
                    });
                }
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

            /// <summary>
            /// Gets or sets optional element attributes keyed by attribute name.
            /// </summary>
            public Dictionary<string, object?>? Attributes { get; set; }
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

            /// <summary>
            /// Gets or sets the child element type (e.g. Numeric, String, Consolidated).
            /// </summary>
            public string? Type { get; set; }

            /// <summary>
            /// Gets or sets optional child element attributes keyed by attribute name.
            /// </summary>
            public Dictionary<string, object?>? Attributes { get; set; }
        }

        /// <summary>
        /// Represents a flattened parent-child relationship row.
        /// </summary>
        public class HierarchyEdge
        {
            /// <summary>Gets or sets the parent element name. Null for root and orphan self-edges.</summary>
            public string? Parent { get; set; }

            /// <summary>Gets or sets the parent element type (e.g. Numeric, String, Consolidated). Null for root and orphan self-edges.</summary>
            public string? ParentType { get; set; }

            /// <summary>Gets or sets the role of the parent element in the hierarchy. Null for root and orphan self-edges.</summary>
            public NodeRole? ParentRole { get; set; }

            /// <summary>Gets or sets the 0-based depth of the parent from its nearest root. Null for root and orphan self-edges.</summary>
            public int? ParentLevel { get; set; }

            /// <summary>Gets or sets the child element name.</summary>
            public string? Child { get; set; }

            /// <summary>Gets or sets the child element type (e.g. Numeric, String, Consolidated).</summary>
            public string? ChildType { get; set; }

            /// <summary>Gets or sets the role of the child element in the hierarchy.</summary>
            public NodeRole ChildRole { get; set; }

            /// <summary>Gets or sets the 0-based depth of the child from its nearest root.</summary>
            public int ChildLevel { get; set; }

            /// <summary>Gets or sets the rollup weight.</summary>
            public double Weight { get; set; }

            /// <summary>Gets or sets the parent element etag.</summary>
            public string? ParentETag { get; set; }

            /// <summary>Gets or sets the child element etag.</summary>
            public string? ChildETag { get; set; }

            /// <summary>Gets or sets the edge/component etag.</summary>
            public string? EdgeETag { get; set; }

            /// <summary>Gets or sets parent element attributes.</summary>
            public Dictionary<string, object?>? ParentAttributes { get; set; }

            /// <summary>Gets or sets child element attributes.</summary>
            public Dictionary<string, object?>? ChildAttributes { get; set; }
        }

        /// <summary>
        /// Represents a flat member entry used to track all dimension members during enrichment.
        /// </summary>
        public class AllMember
        {
            /// <summary>Gets or sets the element name.</summary>
            public string? Name { get; set; }

            /// <summary>Gets or sets the element type (e.g. Numeric, String, Consolidated).</summary>
            public string? Type { get; set; }

            /// <summary>Gets or sets the element etag.</summary>
            public string? ETag { get; set; }

            /// <summary>Gets or sets optional element attributes keyed by attribute name.</summary>
            public Dictionary<string, object?>? Attributes { get; set; }
        }
    }
}
