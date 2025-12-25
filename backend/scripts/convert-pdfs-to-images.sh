#!/bin/bash

# PDF to Image Converter for Receipt Testing

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║              PDF to Image Converter                             ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

RECEIPTS_DIR="${1:-./Receipts}"

if [ ! -d "$RECEIPTS_DIR" ]; then
    echo "❌ Directory not found: $RECEIPTS_DIR"
    exit 1
fi

echo "📂 Receipts directory: $RECEIPTS_DIR"
echo ""

# Count PDFs
PDF_COUNT=$(find "$RECEIPTS_DIR" -type f -name "*.pdf" | wc -l | tr -d ' ')

if [ "$PDF_COUNT" -eq 0 ]; then
    echo "✅ No PDF files found - nothing to convert"
    exit 0
fi

echo "📄 Found $PDF_COUNT PDF file(s) to convert"
echo ""

# Check if sips is available (macOS)
if command -v sips &> /dev/null; then
    echo "🔧 Using macOS 'sips' for conversion..."
    CONVERTER="sips"
elif command -v convert &> /dev/null; then
    echo "🔧 Using ImageMagick 'convert' for conversion..."
    CONVERTER="convert"
else
    echo "❌ No image converter found!"
    echo ""
    echo "Please install one of:"
    echo "  - macOS: sips (built-in)"
    echo "  - ImageMagick: brew install imagemagick"
    exit 1
fi

echo ""

# Convert each PDF
SUCCESS=0
FAILED=0

find "$RECEIPTS_DIR" -type f -name "*.pdf" | while read pdf_file; do
    filename=$(basename "$pdf_file")
    png_file="${pdf_file%.pdf}.png"

    echo "🔄 Converting: $filename"

    if [ "$CONVERTER" = "sips" ]; then
        if sips -s format png "$pdf_file" --out "$png_file" &> /dev/null; then
            echo "   ✅ Created: $(basename "$png_file")"
            ((SUCCESS++))
        else
            echo "   ❌ Failed to convert"
            ((FAILED++))
        fi
    else
        if convert -density 300 "$pdf_file" -quality 100 "$png_file" &> /dev/null; then
            echo "   ✅ Created: $(basename "$png_file")"
            ((SUCCESS++))
        else
            echo "   ❌ Failed to convert"
            ((FAILED++))
        fi
    fi
    echo ""
done

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✨ Conversion complete!"
echo ""
echo "📊 Summary:"
echo "   Total PDFs: $PDF_COUNT"
echo ""
echo "💡 Tip: You can now run the extraction tester on the PNG files"
echo "        ./run-extraction-test.sh"
