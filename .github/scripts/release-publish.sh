#!/usr/bin/env bash
# semantic-release publish step: push the packed packages (and their snupkg symbols)
# to NuGet.org. Requires NUGET_KEY in the environment.
set -euo pipefail

dotnet nuget push "artifacts/packages/*.nupkg" \
  --api-key "${NUGET_KEY}" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
