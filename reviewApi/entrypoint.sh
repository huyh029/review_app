#!/bin/bash
set -e

# Apply migrations only when requested (set RUN_MIGRATIONS=true)
PROJECT_PATH="/app/reviewApi.csproj"
if [ "$RUN_MIGRATIONS" = "true" ]; then
  cd /app
  dotnet ef database update --project "$PROJECT_PATH" --startup-project "$PROJECT_PATH" --no-build
fi

# Run published app
cd /out
exec dotnet reviewApi.dll
