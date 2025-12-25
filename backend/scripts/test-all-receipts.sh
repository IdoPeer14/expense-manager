#!/bin/bash

# Test all receipts in the Receipts folder

EMAIL="ocr-test@example.com"
PASSWORD="TestPassword123"
BASE_URL="https://expense-manager-api-picj.onrender.com"
RECEIPTS_DIR="/Users/idopeer/Projects/expense-manager/Recipts"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Extract JSON field
extract_json_field() {
    echo "$1" | grep -o "\"$2\":\"[^\"]*\"" | sed "s/\"$2\":\"\([^\"]*\)\"/\1/"
}

echo -e "${BLUE}=========================================${NC}"
echo -e "${BLUE}Testing All Receipts${NC}"
echo -e "${BLUE}=========================================${NC}"
echo ""

# Login once
echo "Logging in..."
LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n1)
BODY=$(echo "$LOGIN_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    TOKEN=$(extract_json_field "$BODY" "token")
    echo -e "${GREEN}✓${NC} Logged in successfully"
else
    echo -e "${RED}✗${NC} Login failed"
    exit 1
fi
echo ""

# Counter for results
TOTAL=0
SUCCESS=0
FAILED=0

# Loop through all PDF files
shopt -s nullglob  # Handle case where no PDFs exist
for FILE in $RECEIPTS_DIR/*.pdf; do
    if [ ! -f "$FILE" ]; then
        continue
    fi
    FILENAME=$(basename "$FILE")
    TOTAL=$((TOTAL + 1))

    echo -e "${YELLOW}=========================================${NC}"
    echo -e "${YELLOW}Testing: $FILENAME${NC}"
    echo -e "${YELLOW}=========================================${NC}"

    # Upload
    UPLOAD_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents" \
        -H "Authorization: Bearer $TOKEN" \
        -F "file=@$FILE")

    HTTP_CODE=$(echo "$UPLOAD_RESPONSE" | tail -n1)
    BODY=$(echo "$UPLOAD_RESPONSE" | sed '$d')

    if [ "$HTTP_CODE" != "200" ]; then
        echo -e "${RED}✗ Upload failed${NC}"
        echo "Response: $BODY"
        FAILED=$((FAILED + 1))
        echo ""
        continue
    fi

    DOCUMENT_ID=$(extract_json_field "$BODY" "documentId")
    echo "Document ID: $DOCUMENT_ID"

    # Analyze
    echo "Running OCR..."
    ANALYZE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents/$DOCUMENT_ID/analyze" \
        -H "Authorization: Bearer $TOKEN")

    HTTP_CODE=$(echo "$ANALYZE_RESPONSE" | tail -n1)
    BODY=$(echo "$ANALYZE_RESPONSE" | sed '$d')

    if [ "$HTTP_CODE" = "200" ]; then
        echo -e "${GREEN}✓ OCR Success!${NC}"
        echo ""
        echo "Extracted Data:"
        echo "---------------"

        # Pretty print if available
        if command -v python3 &> /dev/null; then
            echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
        else
            echo "$BODY"
        fi

        SUCCESS=$((SUCCESS + 1))
    else
        echo -e "${RED}✗ OCR Failed${NC}"
        echo "Response: $BODY"
        FAILED=$((FAILED + 1))
    fi

    echo ""
done

# Summary
echo -e "${BLUE}=========================================${NC}"
echo -e "${BLUE}Summary${NC}"
echo -e "${BLUE}=========================================${NC}"
echo "Total receipts: $TOTAL"
echo -e "${GREEN}Successful: $SUCCESS${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo ""
