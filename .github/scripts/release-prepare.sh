#!/usr/bin/env bash
# semantic-release prepare step: stamp the computed version and pack all packages.
# Runs on ubuntu-latest (GNU sed). Invoked as: release-prepare.sh <version>
set -euo pipefail

version="$1"

# Keep the repo's default version in sync with the release
# (committed back to the branch by @semantic-release/git).
sed -i "s|<Version>[^<]*</Version>|<Version>${version}</Version>|" src/Directory.Build.props

rm -rf artifacts/packages
for project in src/*/*.csproj; do
  dotnet pack "$project" -c Release \
    -p:Version="${version}" \
    -p:ContinuousIntegrationBuild=true \
    -o artifacts/packages
done
