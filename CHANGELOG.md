# Changelog

Each release's notes. The Publish workflow takes the section for the
version it releases as the GitHub release's body, so a section is written
here before the version is tagged.

The sections up to 1.1.1.1 were gathered from the README's "Version X
Update" notes and the nuget.org version history, with the date nuget.org
records for each upload. A version neither describes says so.

## [1.1.1.2] - 2026-10-01

* No change to the library's behaviour.
* The README's examples all compile now, and it documents the Planning Analytics Workspace API, `RunProcessWithPollingAsync`, the raw-JSON queries and `hierarchyName`. Several XML doc comments are corrected.
* The package is built in CI from the tagged commit, scanned for vulnerabilities and malware, and published through nuget.org trusted publishing. The GitHub release carries the package's signed build provenance, which `gh attestation verify` checks against the release's copy.

## [1.1.1.1] - 2026-08-12

* Chunking in `WriteCubeCellValuesBatchAsync` is now opt-in. By default every cell goes in one request; pass `useChunks: true` to split the cells into requests of `chunkSize` cells (default 5000).

## [1.1.1] - 2026-08-12

* Update chunking behavior in `WriteCubeCellValuesBatchAsync`.

## [1.1.0] - 2026-05-01

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
* `ParentRole` and `ParentLevel` are nullable, null on self-edges emitted for roots and orphans.
* `ToEdges()` now emits a null-parent self-edge for every `Root` and `Orphan` so that every dimension member appears as `Child` on at least one edge.
* Added `AllMembers` to `DimensionHierarchyModel`: full flat member list populated during enrichment, enabling root/orphan detection without an extra API call.

## [1.0.19.3] - 2025-10-21

No notes were recorded.

## [1.0.19.2] - 2025-10-21

No notes were recorded.

## [1.0.19.1] - 2025-10-21

No notes were recorded.

## [1.0.19] - 2025-09-22

* New Method "WriteCubeCellValuesBatchAsync": Supports writing to multiple cells in a single call.

## [1.0.18.3] - 2025-01-09

No notes were recorded.

## [1.0.18.2] - 2024-09-11

No notes were recorded.

## [1.0.18.1] - 2024-09-11

No notes were recorded.

## [1.0.18] - 2024-09-11

* Increased number of dimension+element parameters to 20 for QueryCellAsync method.
* Methods that create a cellset on the TM1 server will now send a cellset delete API call to clear the cellset from memory after the data has been read.

## [1.0.17] - 2024-06-28

* Added a new method RunProcessWithPollingAsync to run a TI process and poll for completion.
* RunProcessAsync and RunProcessWithPollingAsync now return a string with the process status (ProcessExecuteStatusCode).

## [1.0.16] - 2024-06-27

No notes were recorded.

## [1.0.15] - 2023-09-28

No notes were recorded.

## [1.0.14] - 2023-09-28

No notes were recorded.

## [1.0.13] - 2023-09-27

* Updated async method names to use MethodName+Async naming convention.
* Updated readme examples to use the await keyword.
* Added XML documentation to all public classes and class members.

## [1.0.12] - 2023-09-22

* Modified constructor on TM1SharpConfig class to accept parameter for ignoring SSL certificate errors (default false).

## [1.0.11] - 2023-03-10

No notes were recorded.

## [1.0.10] - 2023-02-08

No notes were recorded.

## [1.0.9] - 2023-01-11

No notes were recorded.

## [1.0.8] - 2023-01-10

No notes were recorded.

## [1.0.7] - 2023-01-04

No notes were recorded.

## [1.0.6] - 2022-12-29

No notes were recorded.

## [1.0.5] - 2022-12-14

No notes were recorded.

## [1.0.4] - 2022-12-14

No notes were recorded.

## [1.0.3] - 2022-12-14

No notes were recorded.

## [1.0.2] - 2022-12-13

No notes were recorded.

## [1.0.1] - 2022-12-07

No notes were recorded.

## [1.0.0] - 2022-12-07

No notes were recorded.
