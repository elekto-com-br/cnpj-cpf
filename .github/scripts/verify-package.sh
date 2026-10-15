#!/usr/bin/env bash
# Checks what `dotnet pack` wrote to the folder given, the same way in CI and in the publish
# workflow, before anything is shipped: the package and its symbol package at the csproj version and
# nothing else, the assembly with its XML documentation and its symbols for every target framework
# the csproj declares, the README and the icon, and, in GitHub Actions, the commit it was built from.
set -euo pipefail

DIST=${1:?usage: verify-package.sh <folder holding the .nupkg and the .snupkg>}
ID=Elekto.BrazilianDocuments
CSPROJ=src/${ID}/${ID}.csproj

VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$CSPROJ" | head -1)
FRAMEWORKS=$(sed -n 's:.*<TargetFrameworks>\(.*\)</TargetFrameworks>.*:\1:p' "$CSPROJ" | head -1)
if [ -z "$VERSION" ] || [ -z "$FRAMEWORKS" ]; then
  echo "::error file=${CSPROJ}::Could not read <Version> and <TargetFrameworks>."
  exit 1
fi

NUPKG=${DIST}/${ID}.${VERSION}.nupkg
SNUPKG=${DIST}/${ID}.${VERSION}.snupkg

mapfile -t PACKAGES < <(find "$DIST" -maxdepth 1 \( -name '*.nupkg' -o -name '*.snupkg' \))
if [ "${#PACKAGES[@]}" -ne 2 ] || [ ! -f "$NUPKG" ] || [ ! -f "$SNUPKG" ]; then
  echo "::error::Expected exactly ${ID}.${VERSION}.nupkg and .snupkg in ${DIST}, found: ${PACKAGES[*]:-nothing}."
  exit 1
fi

# Each listing is written to a file before it is searched: grep -q on a pipe exits at the first
# match, and with pipefail the writer it leaves behind would fail the check
WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT

unzip -Z1 "$NUPKG" > "$WORK/package.txt"
unzip -Z1 "$SNUPKG" > "$WORK/symbols.txt"

in_package() { grep -qxF "$1" "$WORK/package.txt" || { echo "::error::The package lacks $1."; exit 1; }; }
in_symbols() { grep -qxF "$1" "$WORK/symbols.txt" || { echo "::error::The symbol package lacks $1."; exit 1; }; }

IFS=';' read -ra TFMS <<< "$FRAMEWORKS"
for tfm in "${TFMS[@]}"; do
  in_package "lib/${tfm}/${ID}.dll"
  in_package "lib/${tfm}/${ID}.xml"
  in_symbols "lib/${tfm}/${ID}.pdb"
done
in_package README.md
in_package elekto_logo_sq.300x300.png

# Source Link writes the commit it built from into the nuspec; in GitHub Actions that must be the
# commit the workflow checked out
if [ -n "${GITHUB_SHA:-}" ]; then
  unzip -p "$NUPKG" "${ID}.nuspec" > "$WORK/package.nuspec"
  grep -qF "commit=\"${GITHUB_SHA}\"" "$WORK/package.nuspec" \
    || { echo "::error::The nuspec does not name the commit ${GITHUB_SHA} as the source of the package."; exit 1; }
fi

echo "${ID} ${VERSION} holds the assembly, its documentation and symbols for ${FRAMEWORKS}, the README and the icon."
