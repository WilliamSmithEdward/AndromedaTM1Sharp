# AndromedaTM1Sharp
Author: William Smith  
E-Mail: williamsmithe@icloud.com

## Version 1.1.0 Update
* New Method "QueryDimensionMembersJsonAsync": Query members (elements) of a dimension hierarchy as raw JSON.
* New Method "QueryDimensionMembersAsync": Query members (elements) of a dimension hierarchy as a typed dimension list model.
* New Parser "DimensionListJSONParser": Converts dimension members JSON into a DimensionListModel.
* New Method "QueryDimensionHierarchyRollupJsonAsync": Query parent/child rollup structure and weights as raw JSON.
* New Method "QueryDimensionHierarchyRollupAsync": Query parent/child rollup structure and weights as a typed model.
* New Parser "DimensionHierarchyJSONParser": Converts hierarchy rollup JSON into a DimensionHierarchyModel.
* New Helper "ToEdges()": Flattens hierarchy rollups into Parent/Child/Weight rows.
* Improved rollup handling: Supports both TM1 "Edges" and "Elements/Components" payload shapes.
* Added optional ETag metadata fields to dimension members and hierarchy rollup models.
* Improved rollup diagnostics: throws explicit exceptions on REST/OData error payloads.
* ETag enrichment: rollup ParentETag/ChildETag are populated from members query when Edges payload omits etags.
* Added dimension member attribute support: include all attributes (bool) or include only selected attribute names (missing names are ignored).
* Added rollup attribute support: parent/child attributes can be enriched and returned in `ToEdges()` as `ParentAttributes` / `ChildAttributes`.
* Verified JSON alignment against TM1 server payloads for Edges, Elements, and Attributes shapes.
* Standardized query options pattern with `DimensionQueryOptions` for member and rollup queries.
* Added `ParentType` / `ChildType` to `HierarchyEdge`: element type (Numeric, String, Consolidated) for each side of an edge.
* Added `NodeRole` enum to `HierarchyEdge`: classifies each node as `Root` (consolidation with no parent), `Member` (consolidation that is also a child), `Leaf` (never a parent), or `Orphan` (no parent and no children).
* Added `ParentLevel` / `ChildLevel` to `HierarchyEdge`: 0-based depth from the nearest root, computed via BFS across the full hierarchy.
* `ParentRole` and `ParentLevel` are nullable — null on self-edges emitted for roots and orphans.
* `ToEdges()` now emits a null-parent self-edge for every `Root` and `Orphan` so that every dimension member appears as `Child` on at least one edge.
* Added `AllMembers` to `DimensionHierarchyModel`: full flat member list populated during enrichment, enabling root/orphan detection without an extra API call.

## Reading a value from a single cube cell
Example of reading the value of a single cell from a cube.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

List<string> lookupValues = new List<string>()
{
    "Lookup_Value_01",
    "Lookup_Value_02",
    "Lookup_Value_03"
};

lookupValues.ForEach(async x =>
{
    var result = await TM1RestAPI.QueryCellAsync(tm1Config, "Cube_Name", "Dimension_01", x, "Dimension_02", "Element_02");

    Console.WriteLine(result);
});
```

## Reading from an MDX query
Example of running an MDX query to return data. Escape double quote with \\\\\\\"VALUE\\\\\\\" (send literal \\\"VALUE\\\").
See https://www.ibm.com/docs/en/planning-analytics/2.0.0?topic=data-cellsets for details on JSON structure.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

string mdx = "SELECT {[DIMENSION1].[HIERARCHY].[ELEMENT]} ON 0, {[DIMENSION2].[HIERARCHY].[ELEMENT]} ON 1 FROM [YourCube]";

var content = await TM1RestAPI.QueryMDXAsync(tm1Config, mdx);

Console.WriteLine(content);
```

## Reading from a cube view
Example of reading a cellset from a cube view.  
See https://www.ibm.com/docs/en/planning-analytics/2.0.0?topic=data-cellsets for details on JSON structure.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var content = await TM1RestAPI.QueryViewAsync(tm1Config, "YourCube", "YourView");

Console.WriteLine(content);
```

## Converting cellset JSON to System.Data.DataTable
Example of deserializing and converting the JSON return from a View / MDX query to a DataTable.  
Currently supports multiple hierarchy levels on rows.

```csharp
using AndromedaTM1Sharp;
using System.Data;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

string mdx = @"Your_MDX_Statement";

var content = await TM1RestAPI.QueryMDXAsync(tm1Config, mdx);

var model = CellsetJSONParser.ParseIntoObject(content);

var dt = model?.ToDataTable();

foreach (DataRow row in dt.Rows)
{
    foreach (DataColumn column in dt.Columns)
    {
        Console.WriteLine(column.ColumnName + ": " + row[column]);
    }

    Console.WriteLine();
}
```

## Writing to a cube
Example of writing a list of values to a cube, while iterating through one dimension and keeping other dimensions constant. \
** Now supports batch writing multiple cells in a single call. Use WriteCubeCellValuesBatchAsync() **

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var cubeUpdateKVPList = new List<KeyValuePair<string, string>>()
{
    new KeyValuePair<string, string>("9208", "value1"),
    new KeyValuePair<string, "string>("9209", "value2"),
    new KeyValuePair<string, string>("9210", "value3"),
    new KeyValuePair<string, string>("9211", "value4"),
    new KeyValuePair<string, string>("9212", "value5")
};

var cellReferenceList = new List<CellReference>();

cubeUpdateKVPList.ForEach(x =>
{
    cellReferenceList.Add(
        new CellReference(new List<ElementReference>()
        {
            new ElementReference("REGION", "REGION", "Massachusets"),
            new ElementReference("MONTH", "MONTH", x.Key),
            new ElementReference("PROJECT", "PROJECT", "PROJECT NAME")
        }, x.Value
    ));
});

await TM1RestAPI.WriteCubeCellValueAsync(tm1Config, "YourCube", cellReferenceList);
```

## Writing to a cube in batch
Example of writing a large number of cells in chunks using `WriteCubeCellValuesBatchAsync`. Defaults to 5000 cells per batch request.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var cellReferenceList = new List<CellReference>();

// Build as many cell references as needed
cellReferenceList.Add(
    new CellReference(new List<ElementReference>()
    {
        new ElementReference("REGION", "REGION", "Massachusets"),
        new ElementReference("MONTH", "MONTH", "9208"),
        new ElementReference("PROJECT", "PROJECT", "PROJECT NAME")
    }, "42"
));

await TM1RestAPI.WriteCubeCellValuesBatchAsync(tm1Config, "YourCube", cellReferenceList);

// Custom chunk size
await TM1RestAPI.WriteCubeCellValuesBatchAsync(tm1Config, "YourCube", cellReferenceList, chunkSize: 1000);
```

## Querying a list of cubes
Example of reading a list of cubes from the TM1 server.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var content = await TM1RestAPI.QueryCubeListAsync(tm1Config);

Console.WriteLine(content);
```

## Querying the dimensions of a cube
Example of querying the dimensions of a cube.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var content = await TM1RestAPI.QueryCubeDimensionsAsync(tm1Config, "YourCube");

Console.WriteLine(content);
```

## Querying members of a dimension
Example of querying members (elements) from a dimension hierarchy.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension");

model?.Value?.ForEach(x =>
{
    Console.WriteLine($"{x?.Name} ({x?.Type})");
});
```

Example with all attributes included.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension", includeAttributes: true);

model?.Value?.ForEach(x =>
{
    Console.WriteLine($"{x?.Name}: {string.Join(", ", x?.Attributes?.Select(a => $"{a.Key}={a.Value}") ?? [])}");
});
```

Example with selected attribute names only (missing names are ignored).

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension", ["Caption", "Active"]);

model?.Value?.ForEach(x =>
{
    Console.WriteLine($"{x?.Name}: {string.Join(", ", x?.Attributes?.Select(a => $"{a.Key}={a.Value}") ?? [])}");
});
```

## Querying dimension hierarchy rollups
Example of querying parent/child relationships and rollup weights.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "YourDimension");

var edges = model?.ToEdges() ?? [];

edges.ForEach(edge =>
{
    Console.WriteLine($"{edge.Parent} -> {edge.Child} (Weight: {edge.Weight})");
});
```

Example with all parent/child attributes included on edges.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "YourDimension", includeAttributes: true);

var edges = model?.ToEdges() ?? [];

edges.ForEach(edge =>
{
    var parentCaption = edge.ParentAttributes is not null && edge.ParentAttributes.TryGetValue("Caption", out var p) ? p : null;
    var childCaption = edge.ChildAttributes is not null && edge.ChildAttributes.TryGetValue("Caption", out var c) ? c : null;

    Console.WriteLine($"{edge.Parent} [{parentCaption}] -> {edge.Child} [{childCaption}] (Weight: {edge.Weight})");
});
```

Example with selected attribute names only (missing names are ignored).

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(
    tm1Config,
    "YourDimension",
    ["Caption", "Active", "ProjName"]);

var edges = model?.ToEdges() ?? [];

edges.ForEach(edge =>
{
    Console.WriteLine($"{edge.Parent} -> {edge.Child} | ParentAttrs: {edge.ParentAttributes?.Count ?? 0}, ChildAttrs: {edge.ChildAttributes?.Count ?? 0}");
});
```

Example using node role, type, and level to understand hierarchy structure.
Roots and orphans appear as null-parent self-edges so every member is reachable as `Child`.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var model = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "YourDimension");

var edges = model?.ToEdges() ?? [];

edges.ForEach(edge =>
{
    var parentLabel = edge.Parent is null ? "[none]" : $"[{edge.ParentRole} L{edge.ParentLevel}] {edge.Parent}";
    Console.WriteLine($"{parentLabel} -> [{edge.ChildRole} L{edge.ChildLevel}] {edge.Child} ({edge.ChildType})");
});
```

`NodeRole` values:
- `Root` — a consolidation that has no parent in the hierarchy (top-level); appears as both a parent in normal edges and as `Child` on a null-parent self-edge
- `Member` — a consolidation that is also a child of another consolidation (mid-level)
- `Leaf` — an element that is never a parent (bottom-level)
- `Orphan` — no parent and no children; appears only as `Child` on a null-parent self-edge

`ParentRole` and `ParentLevel` are nullable — they are `null` on self-edges emitted for roots and orphans.  
`ParentLevel` / `ChildLevel` are 0-based depths from the nearest root, computed via BFS.

## Running a Turbo Integrator process
Example of running a TI process on the TM1 server. Expected return payload is:  
{"@odata.context":"../$metadata#ibm.tm1.api.v1.ProcessExecuteResult","ProcessExecuteStatusCode":"CompletedSuccessfully"}

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var content = await TM1RestAPI.RunProcessAsync(tm1Config, "_CreateCubeProcess", new Dictionary<string, string>() { { "CubeName", "_NewCubeCreatedbyRestAPI" } });

Console.WriteLine(content);