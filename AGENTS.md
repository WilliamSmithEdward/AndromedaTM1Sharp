# Notes for agents

<!-- repo-standards:begin. Copied from WilliamSmithEdward/repo-standards, templates/agents/AGENTS-block.md. Change it there; the weekly rescan fails a copy that differs. -->
## Releases, CI and security

These rules are the same in every WilliamSmithEdward repository.

- **How a release happens here:** pushing a `vX.Y.Z` tag runs Publish, which builds the release files in CI and creates the GitHub release with them, their signed provenance and the security reports. Any other step, such as a marketplace upload, is described elsewhere in this file.
- **Starting a workflow by hand never releases anything.** Publish and every
  release report are dry runs when started with `gh workflow run` or the Run
  workflow button. They build, scan and assemble the release files exactly
  as a release would, and upload them as the `release-preview` artifact
  instead. Run one after changing anything on the release path:
  `gh workflow run <file> --ref main`, then
  `gh run download <run-id> -n release-preview`.
- **Do not create, publish, edit or delete a release or a `v*` tag** unless
  the owner asks for it. A `v*` tag cannot be moved or deleted once pushed.
- **Every change to `main` goes through a pull request** that passes CI
  passed, Security passed and Malware scan passed. No one can push to `main`
  directly or skip the checks, admins included. Push a branch, open a pull
  request, and let it merge itself: `gh pr merge --auto --squash <number>`.
- **Pins.** Actions by full commit SHA with the version as a comment. Images
  by digest, in `.github/security/<tool>/Dockerfile`. Python tools from the
  hash-locked `.github/requirements/<purpose>.txt`, compiled from the `.in`
  beside it with
  `uv pip compile <purpose>.in --universal --generate-hashes --python-version 3.12 -o <purpose>.txt`.
  Runners are named releases, never `-latest`.
- **Updates merge themselves.** Dependabot and the Update YARA rules workflow
  open pull requests that merge once the three checks pass, except a
  third-party major version, which waits for the owner. Leave them alone
  unless asked.
- **A scanner finding is fixed or accepted with a written reason** in the
  repository's accepted list. Never silence a scanner without one.
<!-- repo-standards:end -->

## This repository

AndromedaTM1Sharp is a .NET client library for the IBM TM1 / Planning
Analytics REST API, published to nuget.org as `AndromedaTM1Sharp`. What an
agent working here must not break:

- **The release path.** A release starts from a `vX.Y.Z` tag that matches
  `PackageVersion` in `AndromedaTM1Sharp/AndromedaTM1Sharp.csproj`; Publish
  refuses any other. Its notes are the version's section of `CHANGELOG.md`
  (`## [X.Y.Z] - date`), written before the tag is pushed; without one the
  release fails. The package goes to nuget.org through trusted publishing:
  nuget.org's policy is bound to `publish.yml` and the `nuget` environment,
  so both keep their names, and no API key is stored anywhere.
- **The lock file.** Restores run with `--locked-mode` against
  `AndromedaTM1Sharp/packages.lock.json`. A new or changed package reference
  is restored without it once, and the updated lock file committed with it.
- **No tests run here.** Every call needs a live TM1 server, which CI does
  not have. CI checks that the library builds with no warnings and that the
  package holds the dll, the XML docs, README.md and the icon. Do not run
  anything that connects to a TM1 or Planning Analytics server.
- **No server details in the repository.** Never commit a server address, a
  user name, a password or an environment name, in code, samples, tests or
  logs. The README samples use placeholders such as `YourTM1Server`.
