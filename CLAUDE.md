# Elekto.BrazilianDocuments

C# library that validates, parses and formats Brazilian CPF and CNPJ numbers, including the
alphanumeric CNPJ, published on NuGet as `Elekto.BrazilianDocuments`. It targets net8.0, net9.0,
net10.0 and netstandard2.0 from one source, with no conditional compilation; for netstandard2.0 it
depends on the System.Text.Json and System.Runtime.CompilerServices.Unsafe packages.

## Language

Code, comments, test names, commit messages and documentation are in English, even when the
conversation with the maintainer is in Portuguese. Commit subjects take a prefix, as in the history:
`feat:`, `fix:`, `docs:`, and `build:` for the build, the workflows and the dependencies.

## Line endings

`.gitattributes` has `* text=auto`: the repository stores LF, and Windows checkouts get CRLF. Git
normalizes new files on commit, whichever they use.

## Layout

- `src/Elekto.BrazilianDocuments/`: `Cpf` and `Cnpj`, readonly structs that are valid by
  construction, with their JSON converters and XML serialization (`IXmlSerializable`, for WCF and
  SOAP); `BrazilianDocument`, for fields that may hold either; `BadDocumentException` and
  `DocumentType`. Every file starts with the MIT license header line.
- `tests/Elekto.BrazilianDocuments.Tests/`: NUnit tests, including allocation and throughput checks.
- `benchmarks/Elekto.BrazilianDocuments.Benchmarks/`: BenchmarkDotNet; the performance table in the
  README comes from it.

## Tests

```bash
dotnet build -c Release
dotnet test --no-build -c Release
```

Benchmarks: `dotnet run -c Release --project benchmarks/Elekto.BrazilianDocuments.Benchmarks`.

## Dependencies

Package versions are locked: `Directory.Build.props` turns on `RestorePackagesWithLockFile`, and
each project keeps its `packages.lock.json`. CI and the publish workflow restore with
`--locked-mode`, so they fail when a lock file does not match its project. After changing a
`PackageReference`, run `dotnet restore` and commit the lock files with it; never edit them by hand.

Dependabot (`.github/dependabot.yml`) proposes updates weekly, each a week after its release
(cooldown). Minor and patch updates of the test and benchmark packages come grouped. A major
version, or any update of what the library itself depends on, comes in a pull request of its own:
the target frameworks and the netstandard2.0 dependencies are what users get, so a change to them
belongs in the release notes.

## Workflows

The package runs inside other people's applications, so the workflows keep third-party code away
from anything that can publish:

- Actions are pinned by full commit SHA, with the release in a comment (`# v7.0.1`); never by tag.
- Restore, build, test and pack (NuGet packages with build targets and analyzers, the test adapter)
  run only in jobs with read-only permissions and no secrets. The jobs that get an OIDC token run
  no code from the repository or from NuGet: `publish` (nuget.org, and the attestation) works from
  the packages the build job uploaded, and `coverage` (Codecov) from the coverage report.
- No long-lived secret is stored: nuget.org and Codecov are both reached through OIDC.
- Every job gets the least permissions it needs; the default for both workflows is
  `contents: read`. Checkout keeps no credentials (`persist-credentials: false`).
- Secrets and tokens reach a command through `env`, never interpolated with `${{ }}` into its text.
- `.github/scripts/verify-package.sh` checks the packed `.nupkg` and `.snupkg` the same way in CI
  and at release.

## Releases

1. Set `<Version>` and `<PackageReleaseNotes>` in
   `src/Elekto.BrazilianDocuments/Elekto.BrazilianDocuments.csproj`. The version must equal the
   tag. The release notes (`vX.Y.Z: ...`) show on nuget.org and become the GitHub Release notes.
2. Commit, push to `master`, and wait for CI to pass.
3. Push an annotated tag: `git tag -a vX.Y.Z -m "<what changed>" && git push origin vX.Y.Z`.

The tag runs `.github/workflows/publish.yml`. A build job checks that the tag matches the csproj,
tests and packs with no credentials, and hands the `.nupkg` and `.snupkg` to a publish job, which
attests their build provenance and pushes them to nuget.org through Trusted Publishing, as the
nuget.org user that the repository variable `NUGET_USER` names (the user who created the policy,
not the package owner). No key is stored, so when that login fails, nothing is published. The
publish job runs in the environment `nuget`, which only `v*.*.*` tags can deploy to. Last comes the
GitHub Release, with both packages attached; check one with
`gh attestation verify Elekto.BrazilianDocuments.X.Y.Z.nupkg --repo elekto-com-br/cnpj-cpf`.
