#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

echo "Restoring PearlMetric solution..."
dotnet restore PearlMetric.slnx

echo "Building PearlMetric solution..."
dotnet build PearlMetric.slnx --configuration Release --no-restore
