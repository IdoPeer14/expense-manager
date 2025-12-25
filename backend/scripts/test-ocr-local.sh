#!/bin/bash

# Local OCR Test Script
# Tests document upload and OCR analysis on local development server

set -e  # Exit on error

BASE_URL="http://localhost:8080"
TEST_EMAIL="local-test@example.com"
TEST_PASSWORD="Test123!@#"
TEST_FILE="${1:-../sample_receipt_en.pdf}"

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
echo -e "${BLUE}Local OCR Test${NC}"
echo -e "${BLUE}=========================================${NC}"
echo "Base URL: $BASE_URL"
echo "Test File: $TEST_FILE"
echo ""

# Check if API is running
echo "Checking if API is running..."
if ! curl -s "$BASE_URL/health" > /dev/null 2>&1; then
    echo -e "${RED}✗${NC} API is not running!"
    echo ""
    echo "Please start the API first:"
    echo "  cd backend/ExpenseManager.Api"
    echo "  dotnet run"
    echo ""
    exit 1
fi
echo -e "${GREEN}✓${NC} API is running"
echo ""

# Step 1: Register user
echo -e "${YELLOW}Step 1: Register Test User${NC}"
echo "========================================="
REGISTER_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

HTTP_CODE=$(echo "$REGISTER_RESPONSE" | tail -n1)
BODY=$(echo "$REGISTER_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    TOKEN=$(extract_json_field "$BODY" "token")
    echo -e "${GREEN}✓${NC} User registered successfully"
elif [ "$HTTP_CODE" = "400" ] && [[ "$BODY" == *"already registered"* ]]; then
    echo -e "${YELLOW}⚠${NC} User already exists, logging in..."

    # Login instead
    LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

    HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n1)
    BODY=$(echo "$LOGIN_RESPONSE" | sed '$d')

    if [ "$HTTP_CODE" = "200" ]; then
        TOKEN=$(extract_json_field "$BODY" "token")
        echo -e "${GREEN}✓${NC} Login successful"
    else
        echo -e "${RED}✗${NC} Login failed"
        echo "Response: $BODY"
        exit 1
    fi
else
    echo -e "${RED}✗${NC} Registration failed"
    echo "Response: $BODY"
    exit 1
fi

echo "Token: ${TOKEN:0:50}..."
echo ""

# Step 2: Upload Document
echo -e "${YELLOW}Step 2: Upload Document${NC}"
echo "========================================="
if [ ! -f "$TEST_FILE" ]; then
    echo -e "${RED}✗${NC} File not found: $TEST_FILE"
    exit 1
fi

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

# Step 3: Analyze Document (OCR)
echo -e "${YELLOW}Step 3: Analyze Document (OCR)${NC}"
echo "========================================="
echo "Running OCR analysis..."

ANALYZE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents/$DOCUMENT_ID/analyze" \
    -H "Authorization: Bearer $TOKEN")

HTTP_CODE=$(echo "$ANALYZE_RESPONSE" | tail -n1)
BODY=$(echo "$ANALYZE_RESPONSE" | sed '$d')

echo ""
echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✓${NC} OCR analysis completed successfully!"
    echo ""
    echo "Extracted Data:"
    echo "---------------"

    # Pretty print if python3 is available
    if command -v python3 &> /dev/null; then
        echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
    else
        echo "$BODY"
    fi
else
    echo -e "${RED}✗${NC} OCR analysis failed"
    echo ""
    echo "This helps us debug the issue!"
fi
echo ""

echo -e "${BLUE}=========================================${NC}"
echo -e "${BLUE}Test Complete!${NC}"
echo -e "${BLUE}=========================================${NC}"
