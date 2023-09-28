# AndromedaTM1Sharp
Author: William Smith  
E-Mail: williamsmithe@icloud.com

## Version 1.0.13 Update
* Updated async method names to use MethodName+Async naming convention.
* Updated readme examples to use the await keyword.
* Added XML documentation to all public classes and class members.

## Version 1.0.12 Update
* Modified constructor on TM1SharpConfig class to accept parameter for ignoring SSL certificate errors (default false).

```csharp
public TM1SharpConfig(string tm1ServerURL, string userName, string password, string environment, bool ignoreSSLCertError = false)
{
    ServerHTTPSAddress = tm1ServerURL.TrimEnd('/');
    UserName = userName;
    Password = password;
    Environment = environment;
    IgnoreSSLCertError = ignoreSSLCertError;
}
```

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
Example of writing a list of values to a cube, while iterating through one dimension and keeping other dimensions constant. 

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

## Running a Turbo Integrator process
Example of running a TI process on the TM1 server. Expected return payload is:  
{"@odata.context":"../$metadata#ibm.tm1.api.v1.ProcessExecuteResult","ProcessExecuteStatusCode":"CompletedSuccessfully"}

```csharp
using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://YourTM1Server:YourPort", "tm1UserName", "tm1Password", "YourEnvName");

var content = await TM1RestAPI.RunProcessAsync(tm1Config, "_CreateCubeProcess", new Dictionary<string, string>() { { "CubeName", "_NewCubeCreatedbyRestAPI" } });

Console.WriteLine(content);
```

Icon Designed by Freepik Company S.L. <https://support.freepik.com/>
