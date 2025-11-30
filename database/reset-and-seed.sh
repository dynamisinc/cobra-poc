#!/bin/bash
#
# reset-and-seed.sh - Resets the ChecklistPOC database and applies all seed data
#
# Usage:
#   ./reset-and-seed.sh                    # Default: localhost
#   ./reset-and-seed.sh -s "server_name"   # Custom server
#   ./reset-and-seed.sh --skip-migrations  # Skip EF migrations
#

set -e

# Default values
SERVER_INSTANCE="localhost"
DATABASE_NAME="ChecklistPOC"
SKIP_MIGRATIONS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -s|--server)
            SERVER_INSTANCE="$2"
            shift 2
            ;;
        --skip-migrations)
            SKIP_MIGRATIONS=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(dirname "$SCRIPT_DIR")/src/backend/ChecklistAPI"

echo ""
echo "========================================"
echo "  ChecklistPOC - Database Reset & Seed"
echo "========================================"
echo ""
echo "Server: $SERVER_INSTANCE"
echo "Database: $DATABASE_NAME"
echo ""

# Step 1: Drop and recreate database via EF Core
if [ "$SKIP_MIGRATIONS" = false ]; then
    echo "[1/6] Dropping existing database..."
    cd "$BACKEND_DIR"

    # Try to drop (may fail if doesn't exist)
    dotnet ef database drop --force 2>/dev/null || echo "  Database may not exist (continuing)"
    echo "  Database dropped"

    echo "[2/6] Applying EF Core migrations..."
    if ! dotnet ef database update; then
        echo "ERROR: Migration failed!"
        exit 1
    fi
    echo "  Migrations applied"

    cd "$SCRIPT_DIR"
else
    echo "[1/6] Skipping database drop (--skip-migrations)"
    echo "[2/6] Skipping migrations (--skip-migrations)"
fi

# Step 2: Run seed scripts in order
declare -a SEED_SCRIPTS=(
    "seed-events.sql|Events & Categories"
    "seed-templates.sql|Templates"
    "seed-item-library.sql|Item Library"
    "seed-checklists.sql|Checklists"
    "seed-operational-periods.sql|Operational Periods"
)

STEP_NUM=3
for entry in "${SEED_SCRIPTS[@]}"; do
    IFS='|' read -r FILE NAME <<< "$entry"
    SCRIPT_PATH="$SCRIPT_DIR/$FILE"

    if [ ! -f "$SCRIPT_PATH" ]; then
        echo "[$STEP_NUM/6] Skipping $NAME - file not found"
        ((STEP_NUM++))
        continue
    fi

    echo "[$STEP_NUM/6] Seeding $NAME..."

    if sqlcmd -S "$SERVER_INSTANCE" -d "$DATABASE_NAME" -i "$SCRIPT_PATH" -b > /dev/null 2>&1; then
        echo "  $NAME seeded"
    else
        echo "  WARNING: $FILE had errors"
    fi

    ((STEP_NUM++))
done

# Summary
echo ""
echo "========================================"
echo "  Database Reset Complete!"
echo "========================================"
echo ""

# Quick verification
echo "Verifying seed data..."
VERIFY_QUERY="SELECT 'Events' AS [Table], COUNT(*) AS [Count] FROM Events
UNION ALL SELECT 'Templates', COUNT(*) FROM Templates
UNION ALL SELECT 'ChecklistInstances', COUNT(*) FROM ChecklistInstances
UNION ALL SELECT 'OperationalPeriods', COUNT(*) FROM OperationalPeriods"

sqlcmd -S "$SERVER_INSTANCE" -d "$DATABASE_NAME" -Q "$VERIFY_QUERY" -h -1 -W 2>/dev/null || true

echo ""
echo "To start the API:"
echo "  cd src/backend/ChecklistAPI && dotnet run"
echo ""
echo "API will be available at:"
echo "  http://localhost:5000"
echo "  http://localhost:5000/swagger"
echo ""
