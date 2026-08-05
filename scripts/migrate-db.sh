#!/bin/bash
# ============================================================================
# MediQueue Database Migration Runner (Linux/Docker)
# Usage: ./migrate-db.sh Production
# ============================================================================

set -e

ENVIRONMENT=${1:-Development}
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJECT="${PROJECT_DIR}/MediQueue.API"
INFRASTRUCTURE_PROJECT="${PROJECT_DIR}/MediQueue.Infrastructure"

echo "======================================================================"
echo "🔄 MediQueue Database Migration Runner"
echo "======================================================================"
echo "Environment: $ENVIRONMENT"
echo ""

# Validate
if [ ! -d "$INFRASTRUCTURE_PROJECT" ]; then
    echo "❌ Infrastructure project not found: $INFRASTRUCTURE_PROJECT"
    exit 1
fi

if [ ! -d "$API_PROJECT" ]; then
    echo "❌ API project not found: $API_PROJECT"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet CLI not found"
    exit 1
fi

echo "✅ Validation passed. Starting migration..."
echo ""

cd "$INFRASTRUCTURE_PROJECT"

if [ "$ENVIRONMENT" = "Production" ]; then
    echo "⚠️  PRODUCTION MODE: Using Release configuration"
    dotnet ef database update \
        --project "$INFRASTRUCTURE_PROJECT" \
        --startup-project "$API_PROJECT" \
        --configuration Release \
        --context ClinicDbContext \
        --verbose
else
    echo "🔧 Development mode"
    dotnet ef database update \
        --project "$INFRASTRUCTURE_PROJECT" \
        --startup-project "$API_PROJECT" \
        --context ClinicDbContext \
        --verbose
fi

if [ $? -eq 0 ]; then
    echo ""
    echo "======================================================================"
    echo "✅ Database migration completed successfully!"
    echo "======================================================================"
    exit 0
else
    echo ""
    echo "======================================================================"
    echo "❌ Migration failed"
    echo "======================================================================"
    exit 1
fi