using AndromedaTM1Sharp;

var tm1Config = new TM1SharpConfig("https://antares:5002", "will", "lincdba", "tm1dev", true);

// PROJINFO — multi-root, no orphans expected
Console.WriteLine("=== PROJINFO ===");
var projInfo = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "PROJINFO");
var infoEdges = projInfo?.ToEdges() ?? [];

var infoNullParent = infoEdges.Where(e => e.Parent is null).ToList();
Console.WriteLine($"Total edges: {infoEdges.Count}  |  Null-parent edges: {infoNullParent.Count}");
infoNullParent.ForEach(e =>
    Console.WriteLine($"  [null] -> [{e.ChildRole} L{e.ChildLevel}] {e.Child} ({e.ChildType})"));

Console.WriteLine();

// PROJCODE — large flat hierarchy, roots should appear
Console.WriteLine("=== PROJCODE ===");
var projCode = await TM1RestAPI.QueryDimensionHierarchyRollupAsync(tm1Config, "PROJCODE", true);
var codeEdges = projCode?.ToEdges() ?? [];

var codeNullParent = codeEdges.Where(e => e.Parent is null).ToList();
Console.WriteLine($"Total edges: {codeEdges.Count}  |  Null-parent edges: {codeNullParent.Count}");

var roots   = codeNullParent.Where(e => e.ChildRole == DimensionHierarchyJSONParser.NodeRole.Root).ToList();
var orphans = codeNullParent.Where(e => e.ChildRole == DimensionHierarchyJSONParser.NodeRole.Orphan).ToList();
Console.WriteLine($"  Roots:   {roots.Count}");
Console.WriteLine($"  Orphans: {orphans.Count}");

Console.WriteLine("\nRoot self-edges:");
roots.Take(3).ToList().ForEach(e =>
    Console.WriteLine($"  [null] -> [{e.ChildRole} L{e.ChildLevel}] {e.Child} ({e.ChildType}) | Attrs: {e.ChildAttributes?.Count ?? 0}"));

// Verify every member appears as Child
var allMemberNames = projCode?.AllMembers?.Select(m => m.Name).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
var allChildNames  = codeEdges.Select(e => e.Child).ToHashSet(StringComparer.OrdinalIgnoreCase);
var missing = allMemberNames.Where(n => !allChildNames.Contains(n!)).ToList();
Console.WriteLine($"\nMembers not appearing as Child: {missing.Count}");
