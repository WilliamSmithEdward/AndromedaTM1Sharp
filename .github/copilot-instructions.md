# Copilot Instructions

## Project Guidelines
- Prefer newer C# collection initialization syntax like `Components = []`.
- Keep public methods documented with XML comments after refactors/new overloads.
- For dimension query overloads, keep `hierarchyName` as the least significant parameter at the end, since it usually matches `dimensionName`.
- For all new features, re-run tests in the dev environment using the provided local config and verify against tm1dev on server https://antares:5002. Use the TM1Debug console project located at C:\Users\wsmith\OneDrive - Hearthstone\Repos\_Libraries\AndromedaTM1Sharp\TM1Debug (with a project reference to AndromedaTM1Sharp) to test raw API calls and end-to-end behavior. Leave the TM1Debug project in place. Fully test all code changes against expected output from the dev server and request refreshed credentials when needed instead of storing them. Never store credentials in files; request refreshed credentials from the user/developer when needed.
- All generated code should be deterministic; refactor non-deterministic patterns.
- Do not include fallback logic or fallback handling in code unless the user explicitly asks for it.

## Model Updates
- When adding new fields or properties to models (e.g. HierarchyEdge), always verify and wire them through the full pipeline: parsing, enrichment, and output. New data should be populated unconditionally unless it is explicitly attribute-related (i.e. don't gate non-attribute fields behind includeAttributes).