#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

echo "Building PearlMetric before tests..."
./scripts/build.sh

shopt -s nullglob
test_projects=(tests/**/*Tests*.csproj tests/**/*Test*.csproj)

if ((${#test_projects[@]} == 0)); then
  echo "No test projects found yet. Skipping tests until PM-021."
  exit 0
fi

echo "Running test projects..."
for project in "${test_projects[@]}"; do
  echo "  -> $project"
  dotnet test "$project" --configuration Release --no-build
done
