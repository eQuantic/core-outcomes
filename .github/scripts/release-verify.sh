#!/usr/bin/env bash
# semantic-release verifyConditions step: refuse to start a release without the NuGet
# key — BEFORE anything is analyzed, tagged, committed or published.
set -euo pipefail

if [ -z "${NUGET_KEY:-}" ]; then
  {
    echo "NUGET_KEY is empty — releases are disarmed."
    echo "Provide the NUGET_KEY secret (org-level, or on the 'nuget' GitHub"
    echo "environment, optionally with required reviewers for a manual approval"
    echo "gate), then re-run the workflow."
  } >&2
  exit 1
fi
