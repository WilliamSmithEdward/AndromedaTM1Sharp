# Security policy

## Reporting a vulnerability

Report a vulnerability privately, not in a public issue or pull request:
[open a private report](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/security/advisories/new).
Only the maintainer sees it. Include the AndromedaTM1Sharp version, the
method involved, whether the server is TM1 / Planning Analytics or Planning
Analytics Workspace and its version, and the smallest code, server response
or steps that show it, with server addresses, credentials and private data
removed.

A confirmed vulnerability is fixed in a release on nuget.org, and the
advisory is published with it, crediting you unless you ask otherwise.

## Supported versions

Only the latest release on nuget.org receives security fixes. Older
releases are not maintained separately; update when a fix ships.

## Scope

AndromedaTM1Sharp is a client library. It opens no port, reads and writes
no local file, and starts no process. Each call sends HTTP requests, over
TLS 1.2 when the address is `https://`, to the server address given in
`TM1SharpConfig`: the TM1 REST API with the user
name and password as HTTP Basic authentication, or, for
`PlanningAnalyticsWorkspaceAPI`, the Planning Analytics Workspace form login
followed by its session cookie. It reads data, writes cube cells and runs
TurboIntegrator processes only when the calling code asks it to, and
returns the server's JSON or a model parsed from it.

The input that comes from outside is the server's responses. These count as
vulnerabilities:

- the user name, password or session cookie reaching any host but the
  configured server, or a log line, exception message or return value;
- a server certificate accepted that does not validate, when
  `ignoreSSLCertError` is `false`;
- a server response that makes the library read or write a local file, run
  anything, or send a request the calling code did not ask for.

An exception on a malformed or unexpected response is not a vulnerability
on its own; report it as an issue.

### Certificate checks

`ignoreSSLCertError: true` makes the client accept any server certificate,
including one an attacker presents, and the credentials then travel to
whoever answers. Leave it `false` against any server you do not control the
network path to.

### Names in requests

Cube, view, dimension, hierarchy, element and process names are placed into
the request path as given, without escaping. Pass names from a source you
trust, not text a user typed.

### Credentials

Read the user name and password from configuration the application already
protects, such as environment variables or a secret store, rather than
writing them into source code.

## How the code is checked

Three workflows check every pull request and every push to `main`, and
their gates decide whether a change can merge: **CI passed**,
**Security passed** and **Malware scan passed**. A gate passes only when
every job before it did, and any unexpected finding fails it, whatever its
severity. Security also runs weekly, so new queries, rules and advisories
reach code that has not changed, and Malware scan runs daily, so new
signatures and rules reach files that have not changed.

- **Code:** CodeQL with GitHub's security-extended queries, for C#
  (extracted from a Release build) and GitHub Actions, and Semgrep with the
  default, C#, security-audit, secrets and GitHub Actions rule sets. Semgrep
  scans the library, the workflows that build and publish it, and the
  scripts in `scripts/security` that judge the scans. A `nosemgrep` comment
  cannot hide a finding. Results go to the repository's code scanning.
- **Workflows:** zizmor audits the GitHub Actions workflows; a finding fails
  Security.
- **Dependencies:** `dotnet list package --vulnerable --include-transitive`
  checks the packages `packages.lock.json` resolves, after the same locked
  restore CI builds with, and any known vulnerability fails Security. The
  library has no NuGet dependencies today beyond the .NET runtime.
- **Malware:** ClamAV, with signatures freshclam fetches and verifies on
  every run, and YARA-X, with the YARA Forge rules pinned to a release and
  its SHA-256, scan every file the commit holds and the .nupkg built from it
  with the locked restore, both as the archive and unpacked. On a release
  they scan the very .nupkg that is published. YARA-X runs YARA Forge's full
  rule set. A scan error fails the report as a match does.
- **OpenSSF Scorecard** rates the repository's security practices on every
  change to `main` and weekly, and the README badge shows the result.
  Its Code-Review and Contributors checks assume more than one
  maintainer, such as a second person approving every change, so a
  single-maintainer project cannot score full marks on them. Its Fuzzing
  check finds no C# fuzzer short of OSS-Fuzz or ClusterFuzzLite, so this
  repository has no fuzz workflow.

## Accepted findings

A finding is fixed, or accepted with a written reason in
[.github/security/accepted.toml](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/blob/main/.github/security/accepted.toml)
for CodeQL and Semgrep, or
[.github/security/malware-accepted.toml](https://github.com/WilliamSmithEdward/AndromedaTM1Sharp/blob/main/.github/security/malware-accepted.toml)
for ClamAV and YARA-X. An entry matches on the tool, the rule and the
file, and in accepted.toml also the text of the flagged line, so an edited
line needs another review, and an entry that no longer matches fails the
report. zizmor keeps
its exceptions in `.github/zizmor.yml` or inline beside the line they
excuse, each with its reason.

The current entries: there are none in either list. zizmor's
`self-repository` and `superfluous-actions` rules are turned off in
`.github/zizmor.yml`, each with its reason and when it comes back.

## Pinning and updates

Everything the workflows run is pinned: actions to full commit SHAs,
runners to named OS releases, scanner images to digests, Python tools to
hash-locked lock files, the library's NuGet packages to
`packages.lock.json`, restored in locked mode, the YARA Forge rules to a
release and its SHA-256, and the YARA-X engine to a release and its
SHA-256. `global.json` sets the .NET SDK's floor at 10.0.400 and lets it
roll forward to a newer feature band, so CI builds with the newest .NET 10
SDK. ClamAV's signatures change too often to pin, so freshclam fetches and
verifies them on every run.

Dependabot proposes updates to the GitHub Actions, the Semgrep and ClamAV
images, the hash-locked files in `.github/requirements`, the NuGet packages
and the .NET SDK in `global.json` once a version is a week old, and at once
for a security advisory. The Update YARA rules workflow
proposes new YARA pins in `.github/security/yara.json` each week. A minor
or patch update, and the YARA pull request, merges itself once CI,
Security and Malware scan pass; a third-party major version waits for
review.

## Releases

A pushed `vX.Y.Z` tag builds the .nupkg with the locked restore, checks
that the tag matches `PackageVersion` in the csproj, and runs Security and
Malware scan on the tagged commit and that package. Nothing is published
unless all of them pass. The package goes to nuget.org through trusted
publishing, so no long-lived API key exists to leak. The GitHub release,
titled with the tag, carries the .nupkg,
`AndromedaTM1Sharp-<version>-security-report.md` and
`AndromedaTM1Sharp-<version>-malware-report.md` beside the scan results
they were made from, and the provenance bundle, with the version's section
of `CHANGELOG.md` as its notes. Started by hand, the Publish workflow is
always a dry run and publishes nothing.

### Verifying a download

Releases after 1.1.1.1 carry a GitHub build provenance attestation for the
.nupkg CI built. Check the copy attached to the GitHub release:

```
gh attestation verify AndromedaTM1Sharp.<version>.nupkg --owner WilliamSmithEdward
```

The output names the commit and workflow run that built the file. The
signed bundle is also attached to the release as
`AndromedaTM1Sharp-<version>.sigstore.json`, so the check works without
asking GitHub for it: add `--bundle AndromedaTM1Sharp-<version>.sigstore.json`.

nuget.org adds its own repository signature to every package it serves,
which changes the file, so the copy from nuget.org does not match the
attestation. Check that copy's signature with `dotnet nuget verify`.

## Repository settings

<!-- repo-standards:begin security-settings. Copied from WilliamSmithEdward/repo-standards, templates/security/settings-block.md. Change it there; the weekly rescan fails a copy that differs. -->
- `main` accepts changes only through a pull request that passes
  **CI passed**, **Security passed** and **Malware scan passed**. The
  ruleset has no bypass, for the owner either, and refuses force-pushes and
  deleting the branch.
- A `v*` release tag cannot be moved or deleted once pushed, except by a
  repository admin.
- A workflow that uses an action not pinned to a full commit SHA fails to
  run. Workflow tokens are read-only unless a job is granted more for
  itself.
- Secret scanning with push protection, Dependabot alerts and security
  updates, and private vulnerability reporting are on.
<!-- repo-standards:end -->
