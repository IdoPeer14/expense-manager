#!/bin/bash

# Document Upload & OCR Test Script
# This script tests document upload and OCR analysis

BASE_URL="https://expense-manager-api-picj.onrender.com"

echo "========================================="
echo "Document Upload & OCR Test"
echo "========================================="

# Check if user provided email and password
if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: $0 <email> <password> [pdf_or_image_file]"
    echo ""
    echo "Example:"
    echo "  $0 test@example.com MyPassword123 invoice.pdf"
    echo ""
    exit 1
fi

TEST_EMAIL="$1"
TEST_PASSWORD="$2"
TEST_FILE="${3:-}"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Extract JSON field
extract_json_field() {
    echo "$1" | grep -o "\"$2\":\"[^\"]*\"" | sed "s/\"$2\":\"\([^\"]*\)\"/\1/"
}

echo "Step 1: Login"
echo "========================================="
LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n1)
BODY=$(echo "$LOGIN_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    TOKEN=$(extract_json_field "$BODY" "token")
    echo -e "${GREEN}✓${NC} Login successful"
    echo "Token: ${TOKEN:0:50}..."
else
    echo -e "${RED}✗${NC} Login failed"
    echo "Response: $BODY"
    exit 1
fi
echo ""

# If no file provided, create a test image
if [ -z "$TEST_FILE" ]; then
    echo "Step 2: Creating test image (no file provided)"
    echo "========================================="

    # Check if ImageMagick is installed
    if command -v convert &> /dev/null; then
        TEST_FILE="/tmp/test-invoice.png"
        convert -size 800x600 xc:white \
            -pointsize 40 -fill black \
            -draw "text 50,100 'חשבונית מס'" \
            -draw "text 50,150 'Invoice'" \
            -draw "text 50,250 'Business Name: Test Ltd'" \
            -draw "text 50,300 'Invoice #: 12345'" \
            -draw "text 50,350 'Date: 20/12/2025'" \
            -draw "text 50,450 'Amount: 100.00'" \
            -draw "text 50,500 'VAT (17%): 17.00'" \
            -draw "text 50,550 'Total: 117.00'" \
            "$TEST_FILE"
        echo -e "${GREEN}✓${NC} Created test image: $TEST_FILE"
    else
        echo -e "${YELLOW}⚠${NC} ImageMagick not installed. Skipping file upload test."
        echo "Install with: brew install imagemagick (macOS) or apt-get install imagemagick (Linux)"
        exit 0
    fi
else
    if [ ! -f "$TEST_FILE" ]; then
        echo -e "${RED}✗${NC} File not found: $TEST_FILE"
        exit 1
    fi
    echo "Using file: $TEST_FILE"
fi
echo ""

echo "Step 3: Upload Document"
echo "========================================="
UPLOAD_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents" \
    -H "Authorization: Bearer $TOKEN" \
    -F "file=@$TEST_FILE")

HTTP_CODE=$(echo "$UPLOAD_RESPONSE" | tail -n1)
BODY=$(echo "$UPLOAD_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    DOCUMENT_ID=$(extract_json_field "$BODY" "documentId")
    echo -e "${GREEN}✓${NC} Document uploaded successfully"
    echo "Document ID: $DOCUMENT_ID"
else
    echo -e "${RED}✗${NC} Document upload failed"
    exit 1
fi
echo ""

echo "Step 4: Analyze Document (OCR)"
echo "========================================="
echo "Running OCR analysis (this may take 5-10 seconds)..."

ANALYZE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents/$DOCUMENT_ID/analyze" \
    -H "Authorization: Bearer $TOKEN")

HTTP_CODE=$(echo "$ANALYZE_RESPONSE" | tail -n1)
BODY=$(echo "$ANALYZE_RESPONSE" | sed '$d')

echo ""
echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✓${NC} OCR analysis completed"
    echo ""
    echo "Extracted Data:"
    echo "---------------"

    # Try to pretty-print the extracted fields
    BUSINESS_NAME=$(extract_json_field "$BODY" "businessName")
    INVOICE_NUM=$(extract_json_field "$BODY" "invoiceNumber")
    BUSINESS_ID=$(extract_json_field "$BODY" "businessId")

    [ -n "$BUSINESS_NAME" ] && echo "Business Name: $BUSINESS_NAME"
    [ -n "$INVOICE_NUM" ] && echo "Invoice Number: $INVOICE_NUM"
    [ -n "$BUSINESS_ID" ] && echo "Business ID: $BUSINESS_ID"

    echo ""
    echo "Full Response:"
    echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
else
    echo -e "${RED}✗${NC} OCR analysis failed"
fi
echo ""

echo "Step 5: Create Expense from OCR Data"
echo "========================================="
echo "You can now manually create an expense using the extracted data"
echo "or integrate this into your frontend workflow."
echo ""

echo "========================================="
echo "Test Complete!"
echo "========================================="
