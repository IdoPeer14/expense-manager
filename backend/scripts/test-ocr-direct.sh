#!/bin/bash

# Direct Tesseract OCR Test using command-line tool
# This tests if Tesseract can read the PDF/image file

FILE="${1:-../sample_receipt_en.pdf}"

echo "========================================"
echo "Tesseract OCR Direct Test"
echo "========================================"
echo ""

if [ ! -f "$FILE" ]; then
    echo "❌ File not found: $FILE"
    echo ""
    echo "Usage: $0 [file-path]"
    exit 1
fi

echo "📄 Testing file: $FILE"
echo ""

# Check Tesseract installation
echo "🔍 Tesseract version:"
tesseract --version
echo ""

# Check available languages
echo "🌍 Available languages:"
tesseract --list-langs
echo ""

# Convert PDF to image if needed
WORKING_FILE="$FILE"
TMP_IMAGE=""
if [[ "$FILE" == *.pdf ]]; then
    echo "📄 Converting PDF to image..."
    TMP_IMAGE="/tmp/ocr_input"
    rm -f "${TMP_IMAGE}.png"

    # Convert PDF to PNG using pdftoppm
    if pdftoppm "$FILE" "$TMP_IMAGE" -png -f 1 -singlefile; then
        WORKING_FILE="${TMP_IMAGE}.png"
        echo "✅ PDF converted to image: $WORKING_FILE"
        if [ ! -f "$WORKING_FILE" ]; then
            echo "❌ Converted image not found at $WORKING_FILE"
            exit 1
        fi
    else
        echo "❌ PDF conversion failed"
        exit 1
    fi
    echo ""
fi

# Run OCR
echo "========================================="
echo "🚀 Running OCR (Hebrew + English)..."
echo "========================================="
echo ""

OUTPUT_FILE="/tmp/ocr_output"
rm -f "$OUTPUT_FILE.txt"

# Run Tesseract
if tesseract "$WORKING_FILE" "$OUTPUT_FILE" -l heb+eng 2>&1; then
    echo ""
    echo "========================================="
    echo "✅ OCR SUCCESS!"
    echo "========================================="
    echo ""
    echo "Extracted Text:"
    echo "----------------------------------------"
    cat "$OUTPUT_FILE.txt"
    echo "----------------------------------------"
    echo ""
    echo "Character count: $(wc -c < "$OUTPUT_FILE.txt")"
    echo "Line count: $(wc -l < "$OUTPUT_FILE.txt")"
    echo ""
else
    echo ""
    echo "❌ OCR FAILED"
    echo ""
fi

# Cleanup
rm -f "$OUTPUT_FILE.txt"
[ -n "$TMP_IMAGE" ] && rm -f "${TMP_IMAGE}.png"
