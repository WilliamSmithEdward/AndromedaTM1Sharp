# Copilot Instructions

## Project Guidelines
- Prefer newer C# collection initialization syntax like `Components = []`.
- Keep public methods documented with XML comments after refactors/new overloads.
- For dimension query overloads, keep `hierarchyName` as the least significant parameter at the end, since it usually matches `dimensionName`.
- For all new features, test raw API calls and end-to-end behavior against a TM1 dev server, from a console project kept outside this repository with a project reference to AndromedaTM1Sharp. Fully test all code changes against expected output from the dev server. Never commit a server address, user name, password or environment name: the repository is public. Request refreshed credentials from the user/developer when needed instead of storing them.
- All generated code should be deterministic; refactor non-deterministic patterns.
- Do not include fallback logic or fallback handling in code unless the user explicitly asks for it.

## Model Updates
- When adding new fields or properties to models (e.g. HierarchyEdge), always verify and wire them through the full pipeline: parsing, enrichment, and output. New data should be populated unconditionally unless it is explicitly attribute-related (i.e. don't gate non-attribute fields behind includeAttributes).