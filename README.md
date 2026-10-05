# AndromedaTM1Sharp

[![NuGet version](https://img.shields.io/nuget/v/AndromedaTM1Sharp)](https://www.nuget.org/packages/AndromedaTM1Sharp)
[![Downloads](https://img.shields.io/nuget/dt/AndromedaTM1Sharp)](https://www.nuget.org/packages/AndromedaTM1Sharp)
[![CI](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/ci.yml)
[![Security](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/security.yml/badge.svg?branch=main)](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/security.yml)
[![Malware scan](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/malware-scan.yml/badge.svg?branch=main)](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/actions/workflows/malware-scan.yml)
[![OpenSSF Scorecard](https://img.shields.io/ossf-scorecard/github.com/WilliamSmithEdward/AndromedaTM1Sharp?label=openssf%20score)](https://scorecard.dev/viewer/?uri=github.com/WilliamSmithEdward/AndromedaTM1Sharp)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/blob/main/LICENSE)

Author: William Smith  
E-Mail: williamsmithe@icloud.com

## Version 1.1.1.2 Update
* No change to the library's behaviour. The examples below all compile, more of the API is documented, and the package is now built, scanned and published from CI with signed build provenance. See [CHANGELOG.md](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/blob/main/CHANGELOG.md).

## Configuring the connection
`TM1SharpConfig` holds the connection details every call takes:

```csharp
new TM1SharpConfig(tm1ServerURL, userName, password, environment, ignoreSSLCertError: false)
```

* `tm1ServerURL`: the server's address; a trailing `/` is removed.
* `userName` and `password`: sent with every TM1 REST API request as HTTP Basic authentication.
* `environment`: the TM1 server (environment) name. Only `PlanningAnalyticsWorkspaceAPI` uses it; the `TM1RestAPI` calls ignore it.
* `ignoreSSLCertError` (optional, default `false`): when `true`, the client accepts any server certificate, including an invalid or self-signed one. This removes the protection TLS gives the credentials, so use it only against a test server you trust.

Connections use TLS 1.2.

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

foreach (var x in lookupValues)
{
    var result = await TM1RestAPI.QueryCellAsync(tm1Config, "Cube_Name", "Dimension_01", x, "Dimension_02", "Element_02");

    Console.WriteLine(result);
}
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
Rows may hold several hierarchies; columns support one hierarchy.

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
    new KeyValuePair<string, string>("9209", "value2"),
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
Example of writing a large number of cells using `WriteCubeCellValuesBatchAsync`. By default every cell goes in one request. Chunking is opt-in: pass `useChunks: true` to send the cells in requests of `chunkSize` cells (default 5000).

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

// All cells in one request
await TM1RestAPI.WriteCubeCellValuesBatchAsync(tm1Config, "YourCube", cellReferenceList);

// Chunks of the default 5000 cells
await TM1RestAPI.WriteCubeCellValuesBatchAsync(tm1Config, "YourCube", cellReferenceList, useChunks: true);

// Custom chunk size
await TM1RestAPI.WriteCubeCellValuesBatchAsync(tm1Config, "YourCube", cellReferenceList, useChunks: true, chunkSize: 1000);
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

var model = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension", includeAttributes: false);

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

## Dimension queries: hierarchy, options and raw JSON
Every dimension member and rollup query takes an optional last argument, `hierarchyName`. When it is omitted or blank, the hierarchy with the same name as the dimension is used.

```csharp
var model = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension", includeAttributes: false, hierarchyName: "YourAlternateHierarchy");
```

Each query also has an overload that takes a `DimensionQueryOptions`. `IncludeAttributes` includes all attributes; `AttributeNames` includes only the named ones (missing names are ignored), and setting it includes attributes even when `IncludeAttributes` is `false`.

```csharp
var options = new DimensionQueryOptions { AttributeNames = ["Caption"] };

var members = await TM1RestAPI.QueryDimensionMembersAsync(tm1Config, "YourDimension", options);

var rollup = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "YourDimension", options);
```

To get the server's JSON as a string instead of a parsed model, use the `Json` variants:

* `QueryDimensionMembersJsonAsync`, with the same overloads as `QueryDimensionMembersAsync`: the hierarchy's elements (`Name`, `Type`, and `Attributes` when requested). With `AttributeNames` set, the JSON is re-serialized with only those attributes.
* `QueryDimensionHierarchyRollupJsonAsync`: the hierarchy's `/Edges` response where the server supports it; otherwise the elements with their components and weights. It throws `InvalidOperationException` when both requests fail. It takes only `dimensionName` and `hierarchyName`.

```csharp
var json = await TM1RestAPI.QueryDimensionMembersJsonAsync(tm1Config, "YourDimension", includeAttributes: true);

Console.WriteLine(json);
```

## Running a Turbo Integrator process
Example of running a TI process on the TM1 server. `RunProcessAsync` returns the `ProcessExecuteStatusCode` value from the server's response, such as `CompletedSuccessfully`, not the whole JSON payload.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var status = await TM1RestAPI.RunProcessAsync(tm1Config, "_CreateCubeProcess", new Dictionary<string, object>() { { "CubeName", "_NewCubeCreatedbyRestAPI" } });

Console.WriteLine(status);
```

## Running a Turbo Integrator process with polling
`RunProcessWithPollingAsync` asks the server to run the process asynchronously, then checks for the result once a second until it arrives or `timeoutSeconds` (default 60) checks have been made, when it throws a `TimeoutException`. Like `RunProcessAsync`, it returns the `ProcessExecuteStatusCode` value. Use it for long-running processes: in testing, the server has sometimes returned no JSON to `RunProcessAsync` for those.

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var status = await TM1RestAPI.RunProcessWithPollingAsync(tm1Config, "_LongRunningProcess", timeoutSeconds: 300, parameters: new Dictionary<string, object>() { { "Year", "2024" } });

Console.WriteLine(status);
```

## Planning Analytics Workspace API
`PlanningAnalyticsWorkspaceAPI` calls the Planning Analytics Workspace (PAW) services rather than the TM1 REST API. Each call first logs in through the PAW form login with the configured user name and password, then returns the raw JSON response as a string. `ServerAddress` here is the PAW address, and `environment` names the TM1 server to use.

* `QueryObjectListAsync(tm1Config)`: the server's folders, with control objects and chores.
* `QueryCubeListAsync(tm1Config)`: the server's cubes.
* `QueryCubeDimensionsAsync(tm1Config, "YourCube")`: a cube's dimensions.
* `QueryCubeViewsAsync(tm1Config, "YourCube")`: a cube's views.
* `QueryViewCellSetAsync(tm1Config, "YourCube", "YourView")`: creates a grid for a view and returns its cell set.

```csharp
using AndromedaTM1Sharp;

var pawConfig = new TM1SharpConfig("https://YourPAWServer", "pawUserName", "pawPassword", "YourEnvName");

var content = await PlanningAnalyticsWorkspaceAPI.QueryCubeListAsync(pawConfig);

Console.WriteLine(content);
```