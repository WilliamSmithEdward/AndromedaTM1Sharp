using System.Text.Json;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy using the specified TM1SharpConfig.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A string representing the queried members.</returns>
        public async static Task<string> QueryDimensionMembersJsonAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
        {
            var client = tm1.GetTM1RestClient();

            if (string.IsNullOrWhiteSpace(hierarchyName)) hierarchyName = dimensionName;

            var response = await client.GetAsync(tm1.ServerAddress + "/api/v1/Dimensions('" + dimensionName + "')/Hierarchies('" + hierarchyName + "')/Elements?$select=Name,Type");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Asynchronously queries members (elements) of a dimension hierarchy and parses the result into a dimension list model.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed dimension list model, or null if parsing fails.</returns>
        public async static Task<DimensionListJSONParser.DimensionListModel?> QueryDimensionMembersAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
        {
            var content = await QueryDimensionMembersJsonAsync(tm1, dimensionName, hierarchyName);

            return DimensionListJSONParser.ToDimensionListModel(content);
        }

        /// <summary>
        /// Asynchronously queries hierarchy rollup structure (parent -> child components) as raw JSON.
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
        /// Asynchronously queries hierarchy rollup structure and parses the result into a typed model.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="dimensionName">The name of the dimension.</param>
        /// <param name="hierarchyName">The name of the hierarchy. If omitted, defaults to the dimension name.</param>
        /// <returns>A parsed hierarchy rollup model, or null if parsing fails.</returns>
        public async static Task<DimensionHierarchyJSONParser.DimensionHierarchyModel?> QueryDimensionHierarchyRollupAsync(TM1SharpConfig tm1, string dimensionName, string hierarchyName = "")
        {
            if (string.IsNullOrWhiteSpace(hierarchyName)) hierarchyName = dimensionName;

            var content = await QueryDimensionHierarchyRollupJsonAsync(tm1, dimensionName, hierarchyName);
            var model = ToDimensionHierarchyModel(content);

            if (model is null) return null;

            await EnrichHierarchyModelWithElementEtagsAsync(tm1, dimensionName, hierarchyName, model);

            return model;
        }

        private static async Task EnrichHierarchyModelWithElementEtagsAsync(
            TM1SharpConfig tm1,
            string dimensionName,
            string hierarchyName,
            DimensionHierarchyJSONParser.DimensionHierarchyModel model)
        {
            try
            {
                var membersModel = await QueryDimensionMembersAsync(tm1, dimensionName, hierarchyName);
                var etagsByName = (membersModel?.Value ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x?.Name))
                    .GroupBy(x => x!.Name!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.ETag, StringComparer.OrdinalIgnoreCase);

                foreach (var parent in model.Value ?? [])
                {
                    if (!string.IsNullOrWhiteSpace(parent?.Name) && etagsByName.TryGetValue(parent.Name, out var parentEtag))
                    {
                        parent.ETag ??= parentEtag;
                    }

                    foreach (var component in parent?.Components ?? [])
                    {
                        if (component is null) continue;

                        var childName = component.Name ?? component.Component?.Name;

                        if (string.IsNullOrWhiteSpace(childName)) continue;

                        if (etagsByName.TryGetValue(childName, out var childEtag))
                        {
                            component.Component ??= new DimensionHierarchyJSONParser.HierarchyComponentElement { Name = childName };
                            component.Component.ETag ??= childEtag;
                        }
                    }
                }
            }
            catch
            {
                // Do not fail rollup parsing if etag enrichment call fails.
            }
        }

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
    }
}
