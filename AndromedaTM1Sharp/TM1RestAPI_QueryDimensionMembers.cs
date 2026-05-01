using System.Text.Json;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy as raw JSON.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried members.</returns>
        public async static Task<string> QueryDimensionMembersJsonAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
            => await QueryDimensionMembersJsonAsync(tm1, dimensionName, new DimensionQueryOptions(), hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy as raw JSON, with optional attribute inclusion.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="includeAttributes">Whether to include all attributes.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried members.</returns>
        public async static Task<string> QueryDimensionMembersJsonAsync(TM1SharpConfig tm1, string dimensionName, bool includeAttributes = false, string hierarchyName = "")
            => await QueryDimensionMembersJsonAsync(tm1, dimensionName, new DimensionQueryOptions { IncludeAttributes = includeAttributes }, hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy as raw JSON, including only selected attributes.
        /// Missing attribute names are ignored.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="attributeNames">Attribute names to include.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried members.</returns>
        public async static Task<string> QueryDimensionMembersJsonAsync(TM1SharpConfig tm1, string dimensionName, List<string> attributeNames, string hierarchyName = "")
            => await QueryDimensionMembersJsonAsync(tm1, dimensionName, new DimensionQueryOptions { AttributeNames = attributeNames ?? [] }, hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy as raw JSON using query options.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="options">Member query options.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried members.</returns>
        public async static Task<string> QueryDimensionMembersJsonAsync(TM1SharpConfig tm1, string dimensionName, DimensionQueryOptions options, string hierarchyName = "")
        {
            options ??= new DimensionQueryOptions();

            var includeAttributes = options.IncludeAttributes || options.AttributeNames.Count > 0;
            var client = tm1.GetTM1RestClient();

            if (string.IsNullOrWhiteSpace(hierarchyName)) hierarchyName = dimensionName;

            var url = tm1.ServerAddress + "/api/v1/Dimensions('" + dimensionName + "')/Hierarchies('" + hierarchyName + "')/Elements?$select=Name,Type";

            if (includeAttributes) url += ",Attributes";

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!includeAttributes || options.AttributeNames.Count == 0) return content;

            var model = DimensionListJSONParser.ToDimensionListModel(content);

            if (model?.Value is null) return content;

            var targetAttributes = options.AttributeNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var element in model.Value)
            {
                if (element?.Attributes is null) continue;

                element.Attributes = NormalizeAttributesDictionary(element.Attributes, targetAttributes);
            }

            return JsonSerializer.Serialize(model);
        }

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy and parses the result into a model.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed dimension members model, or null if parsing fails.</returns>
        public async static Task<DimensionListJSONParser.DimensionListModel?> QueryDimensionMembersAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
            => await QueryDimensionMembersAsync(tm1, dimensionName, new DimensionQueryOptions(), hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy and parses the result into a model,
        /// with optional attribute inclusion.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="includeAttributes">Whether to include all attributes.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed dimension members model, or null if parsing fails.</returns>
        public async static Task<DimensionListJSONParser.DimensionListModel?> QueryDimensionMembersAsync(TM1SharpConfig tm1, string dimensionName, bool includeAttributes = false, string hierarchyName = "")
            => await QueryDimensionMembersAsync(tm1, dimensionName, new DimensionQueryOptions { IncludeAttributes = includeAttributes }, hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy and parses the result into a model,
        /// including only selected attributes. Missing attribute names are ignored.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="attributeNames">Attribute names to include.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed dimension members model, or null if parsing fails.</returns>
        public async static Task<DimensionListJSONParser.DimensionListModel?> QueryDimensionMembersAsync(TM1SharpConfig tm1, string dimensionName, List<string> attributeNames, string hierarchyName = "")
            => await QueryDimensionMembersAsync(tm1, dimensionName, new DimensionQueryOptions { AttributeNames = attributeNames ?? [] }, hierarchyName);

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy and parses the result into a model using query options.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="options">Member query options.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed dimension members model, or null if parsing fails.</returns>
        public async static Task<DimensionListJSONParser.DimensionListModel?> QueryDimensionMembersAsync(TM1SharpConfig tm1, string dimensionName, DimensionQueryOptions options, string hierarchyName = "")
        {
            var content = await QueryDimensionMembersJsonAsync(tm1, dimensionName, options, hierarchyName);

            return DimensionListJSONParser.ToDimensionListModel(content);
        }

        /// <summary>
        /// Asynchronously queries hierarchy rollup structure (parent-child edges) as raw JSON.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried hierarchy rollup structure.</returns>
        public async static Task<string> QueryDimensionHierarchyRollupJsonAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
        {
            var client = tm1.GetTM1RestClient();

            if (string.IsNullOrWhiteSpace(hierarchyName)) hierarchyName = dimensionName;

            var edgesUrl = tm1.ServerAddress + "/api/v1/Dimensions('" + dimensionName + "')/Hierarchies('" + hierarchyName + "')/Edges?$select=ParentName,ComponentName,Weight";
            var elementsUrl = tm1.ServerAddress + "/api/v1/Dimensions('" + dimensionName + "')/Hierarchies('" + hierarchyName + "')/Elements?$select=Name,Type&$expand=Components($select=Weight;$expand=Component($select=Name))";

            var response = await client.GetAsync(edgesUrl);

            if (!response.IsSuccessStatusCode)
            {
                response = await client.GetAsync(elementsUrl);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("TM1 hierarchy rollup query failed: " + content);
            }

            return content;
        }

        /// <summary>
        /// Asynchronously queries hierarchy rollups and parses the result into a model.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed hierarchy rollup model, or null if parsing fails.</returns>
        public async static Task<DimensionHierarchyJSONParser.DimensionHierarchyModel?> QueryDimensionHierarchyRollupAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
            => await QueryDimensionHierarchyRollupAsync(tm1, dimensionName, new DimensionQueryOptions(), hierarchyName);

        /// <summary>
        /// Asynchronously queries hierarchy rollups and parses the result into a model with optional attribute inclusion.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="includeAttributes">Whether to include all attributes for parent/child nodes.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed hierarchy rollup model, or null if parsing fails.</returns>
        public async static Task<DimensionHierarchyJSONParser.DimensionHierarchyModel?> QueryDimensionHierarchyRollupAsync(TM1SharpConfig tm1, string dimensionName, bool includeAttributes, string hierarchyName = "")
            => await QueryDimensionHierarchyRollupAsync(tm1, dimensionName, new DimensionQueryOptions { IncludeAttributes = includeAttributes }, hierarchyName);

        /// <summary>
        /// Asynchronously queries hierarchy rollups and parses the result into a model with selected attributes.
        /// Missing attribute names are ignored.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="attributeNames">Attribute names to include.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed hierarchy rollup model, or null if parsing fails.</returns>
        public async static Task<DimensionHierarchyJSONParser.DimensionHierarchyModel?> QueryDimensionHierarchyRollupAsync(TM1SharpConfig tm1, string dimensionName, List<string> attributeNames, string hierarchyName = "")
            => await QueryDimensionHierarchyRollupAsync(tm1, dimensionName, new DimensionQueryOptions { AttributeNames = attributeNames ?? [] }, hierarchyName);

        /// <summary>
        /// Asynchronously queries hierarchy rollups and parses the result into a model using query options.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="options">Rollup query options.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed hierarchy rollup model, or null if parsing fails.</returns>
        public async static Task<DimensionHierarchyJSONParser.DimensionHierarchyModel?> QueryDimensionHierarchyRollupAsync(TM1SharpConfig tm1, string dimensionName, DimensionQueryOptions options, string hierarchyName = "")
        {
            options ??= new DimensionQueryOptions();

            if (string.IsNullOrWhiteSpace(hierarchyName)) hierarchyName = dimensionName;

            var content = await QueryDimensionHierarchyRollupJsonAsync(tm1, dimensionName, hierarchyName);
            var model = ToDimensionHierarchyModel(content);

            if (model is null) return null;

            var includeAttributes = options.IncludeAttributes || options.AttributeNames.Count > 0;

            await EnrichHierarchyModelWithElementMetadataAsync(tm1, dimensionName, hierarchyName, model, includeAttributes, options.AttributeNames);

            return model;
        }

        private static async Task EnrichHierarchyModelWithElementMetadataAsync(
            TM1SharpConfig tm1,
            string dimensionName,
            string hierarchyName,
            DimensionHierarchyJSONParser.DimensionHierarchyModel model,
            bool includeAttributes,
            List<string> attributeNames)
        {
            var membersJson = await QueryDimensionMembersJsonAsync(tm1, dimensionName, includeAttributes, hierarchyName);
            var membersByName = ParseElementMetadataMap(membersJson, includeAttributes);

            var targetAttributes = (attributeNames ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Populate AllMembers unconditionally so ToEdges() can emit root/orphan self-edges
            model.AllMembers = [.. membersByName
                .Select(kvp => new DimensionHierarchyJSONParser.AllMember
                {
                    Name       = kvp.Key,
                    Type       = kvp.Value.Type,
                    ETag       = kvp.Value.ETag,
                    Attributes = includeAttributes
                        ? NormalizeAttributesDictionary(kvp.Value.Attributes, targetAttributes.Count > 0 ? targetAttributes : null)
                        : null
                })];

            foreach (var parent in model.Value ?? [])
            {
                if (string.IsNullOrWhiteSpace(parent?.Name)) continue;

                if (membersByName.TryGetValue(parent.Name, out var parentMetadata))
                {
                    parent.ETag ??= parentMetadata.ETag;
                    parent.Type ??= parentMetadata.Type;
                    parent.Attributes ??= NormalizeAttributesDictionary(parentMetadata.Attributes, includeAttributes ? targetAttributes : null);
                }

                foreach (var component in parent.Components ?? [])
                {
                    if (component is null) continue;

                    var childName = component.Name ?? component.Component?.Name;

                    if (string.IsNullOrWhiteSpace(childName)) continue;

                    if (membersByName.TryGetValue(childName, out var childMetadata))
                    {
                        component.Component ??= new DimensionHierarchyJSONParser.HierarchyComponentElement { Name = childName };
                        component.Component.ETag ??= childMetadata.ETag;
                        component.Component.Type ??= childMetadata.Type;
                        component.Component.Attributes ??= NormalizeAttributesDictionary(childMetadata.Attributes, includeAttributes ? targetAttributes : null);
                    }
                }
            }
        }

        private static Dictionary<string, ElementMetadata> ParseElementMetadataMap(string membersJson, bool includeAttributes)
        {
            var result = new Dictionary<string, ElementMetadata>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(membersJson)) return result;

            using var doc = JsonDocument.Parse(membersJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("value", out var valueProperty) || valueProperty.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in valueProperty.EnumerateArray())
            {
                if (!item.TryGetProperty("Name", out var nameProperty)) continue;

                var name = nameProperty.GetString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var etag = item.TryGetProperty("@odata.etag", out var etagProperty)
                    ? etagProperty.GetString()
                    : null;

                Dictionary<string, object?>? attributes = null;

                if (includeAttributes)
                {
                    attributes = ExtractAttributesFromJsonElement(item);
                }

                var type = item.TryGetProperty("Type", out var typeProperty) ? typeProperty.GetString() : null;

                result[name] = new ElementMetadata(etag, type, attributes);
            }

            return result;
        }

        private sealed record ElementMetadata(string? ETag, string? Type, Dictionary<string, object?>? Attributes);

        private static DimensionHierarchyJSONParser.DimensionHierarchyModel? ToDimensionHierarchyModel(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorProperty))
            {
                throw new InvalidOperationException("TM1 hierarchy rollup query returned error: " + errorProperty.ToString());
            }

            var model = new DimensionHierarchyJSONParser.DimensionHierarchyModel();

            if (root.TryGetProperty("@odata.context", out var contextProperty))
            {
                model.MetaData = contextProperty.GetString();
            }

            if (!root.TryGetProperty("value", out var valueProperty) || valueProperty.ValueKind != JsonValueKind.Array)
            {
                return model;
            }

            var values = new List<DimensionHierarchyJSONParser.HierarchyElement>();
            var byParent = new Dictionary<string, DimensionHierarchyJSONParser.HierarchyElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in valueProperty.EnumerateArray())
            {
                if (item.TryGetProperty("ParentName", out var parentNameProperty) && item.TryGetProperty("ComponentName", out var componentNameProperty))
                {
                    var parentName = parentNameProperty.GetString();
                    var componentName = componentNameProperty.GetString();
                    var weight = item.TryGetProperty("Weight", out var weightProperty) && weightProperty.ValueKind == JsonValueKind.Number
                        ? weightProperty.GetDouble()
                        : 0;
                    var edgeEtag = item.TryGetProperty("@odata.etag", out var edgeEtagProperty) ? edgeEtagProperty.GetString() : null;

                    if (string.IsNullOrWhiteSpace(parentName) || string.IsNullOrWhiteSpace(componentName)) continue;

                    if (!byParent.TryGetValue(parentName, out var parent))
                    {
                        parent = new DimensionHierarchyJSONParser.HierarchyElement
                        {
                            Name = parentName,
                            Type = "Consolidated",
                            Components = []
                        };

                        byParent[parentName] = parent;
                        values.Add(parent);
                    }

                    parent.Components ??= [];
                    parent.Components.Add(new DimensionHierarchyJSONParser.HierarchyComponent
                    {
                        Name = componentName,
                        Component = new DimensionHierarchyJSONParser.HierarchyComponentElement { Name = componentName },
                        Weight = weight,
                        ETag = edgeEtag
                    });

                    continue;
                }

                var element = new DimensionHierarchyJSONParser.HierarchyElement
                {
                    Name = item.TryGetProperty("Name", out var elementNameProperty) ? elementNameProperty.GetString() : null,
                    Type = item.TryGetProperty("Type", out var typeProperty) ? typeProperty.GetString() : null,
                    ETag = item.TryGetProperty("@odata.etag", out var elementEtagProperty) ? elementEtagProperty.GetString() : null,
                    Attributes = ExtractAttributesFromJsonElement(item),
                    Components = []
                };

                if (item.TryGetProperty("Components", out var componentsProperty) && componentsProperty.ValueKind == JsonValueKind.Array)
                {
                    foreach (var component in componentsProperty.EnumerateArray())
                    {
                        string? componentName = null;
                        string? componentEtag = component.TryGetProperty("@odata.etag", out var componentEtagProperty) ? componentEtagProperty.GetString() : null;
                        string? nestedComponentEtag = null;

                        if (component.TryGetProperty("Name", out var componentNameProperty2))
                        {
                            componentName = componentNameProperty2.GetString();
                        }
                        else if (component.TryGetProperty("Component", out var nestedComponentProperty) &&
                                 nestedComponentProperty.ValueKind == JsonValueKind.Object)
                        {
                            if (nestedComponentProperty.TryGetProperty("Name", out var nestedNameProperty))
                            {
                                componentName = nestedNameProperty.GetString();
                            }

                            if (nestedComponentProperty.TryGetProperty("@odata.etag", out var nestedEtagProperty))
                            {
                                nestedComponentEtag = nestedEtagProperty.GetString();
                            }
                        }

                        var weight = component.TryGetProperty("Weight", out var componentWeightProperty) && componentWeightProperty.ValueKind == JsonValueKind.Number
                            ? componentWeightProperty.GetDouble()
                            : 0;

                        if (string.IsNullOrWhiteSpace(componentName)) continue;

                        element.Components.Add(new DimensionHierarchyJSONParser.HierarchyComponent
                        {
                            Name = componentName,
                            Component = new DimensionHierarchyJSONParser.HierarchyComponentElement
                            {
                                Name = componentName,
                                ETag = nestedComponentEtag
                            },
                            Weight = weight,
                            ETag = componentEtag
                        });
                    }
                }

                values.Add(element);
            }

            model.Value = values;

            return model;
        }

        private static Dictionary<string, object?>? ExtractAttributesFromJsonElement(JsonElement element)
        {
            if (!element.TryGetProperty("Attributes", out var attributesElement) || attributesElement.ValueKind != JsonValueKind.Object)
                return null;

            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in attributesElement.EnumerateObject())
            {
                result[property.Name] = NormalizeJsonValue(property.Value);
            }

            return result.Count > 0 ? result : null;
        }

        private static Dictionary<string, object?>? NormalizeAttributesDictionary(
            Dictionary<string, object?>? attributes,
            HashSet<string>? targetAttributes)
        {
            if (attributes is null) return null;

            IEnumerable<KeyValuePair<string, object?>> source = attributes;

            if (targetAttributes is not null && targetAttributes.Count > 0)
            {
                source = source.Where(kvp => targetAttributes.Contains(kvp.Key));
            }

            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in source)
            {
                result[kvp.Key] = NormalizeJsonValue(kvp.Value);
            }

            return result;
        }

        private static object? NormalizeJsonValue(object? value)
        {
            if (value is not JsonElement jsonElement) return value;

            return jsonElement.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number when jsonElement.TryGetInt32(out var i) => i,
                JsonValueKind.Number when jsonElement.TryGetInt64(out var l) => l,
                JsonValueKind.Number when jsonElement.TryGetDouble(out var d) => d,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => jsonElement.ToString()
            };
        }
    }
}
