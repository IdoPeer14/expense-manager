#!/bin/bash

# Extraction Tester Runner Script

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║          Regex-Based Invoice Extraction Tester                  ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

# Default to backend/Receipts relative to script location
RECEIPTS_DIR="${1:-$SCRIPT_DIR/Receipts}"

echo "📂 Receipts directory: $RECEIPTS_DIR"
echo ""

# Check if directory exists
if [ ! -d "$RECEIPTS_DIR" ]; then
    echo "❌ Directory not found: $RECEIPTS_DIR"
    echo ""
    echo "Creating directory..."
    mkdir -p "$RECEIPTS_DIR"
    echo "✅ Created: $RECEIPTS_DIR"
    echo ""
    echo "Please add receipt files (PDF, PNG, JPG) to this directory and run again."
    exit 1
fi

# Check if directory has files
FILE_COUNT=$(find "$RECEIPTS_DIR" -type f \( -name "*.pdf" -o -name "*.png" -o -name "*.jpg" -o -name "*.jpeg" -o -name "*.tif" -o -name "*.tiff" \) | wc -l | tr -d ' ')

if [ "$FILE_COUNT" -eq 0 ]; then
    echo "❌ No receipt files found in: $RECEIPTS_DIR"
    echo ""
    echo "Supported formats: PDF, PNG, JPG, JPEG, TIF, TIFF"
    echo ""
    echo "Please add some receipt files and run again."
    exit 1
fi

echo "📄 Found $FILE_COUNT receipt file(s)"
echo ""

# Run the tester
cd "$SCRIPT_DIR/ExtractionTester"
echo "🚀 Starting extraction tester..."
echo ""

# Convert to absolute path if relative
if [[ "$RECEIPTS_DIR" != /* ]]; then
    RECEIPTS_DIR="$(cd "$(dirname "$RECEIPTS_DIR")" && pwd)/$(basename "$RECEIPTS_DIR")"
fi

dotnet run "$RECEIPTS_DIR"

EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ Test completed successfully!"
    echo ""
    echo "📊 Results saved to: ExtractionTester/extraction_results.json"
else
    echo "❌ Test failed with exit code: $EXIT_CODE"
fi

exit $EXIT_CODE
