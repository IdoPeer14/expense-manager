#!/bin/bash

# Expense Manager API Test Script
# This script tests all endpoints of the deployed API

BASE_URL="https://expense-manager-api-picj.onrender.com"
TEST_EMAIL="test-$(date +%s)@example.com"
TEST_PASSWORD="Test123!@#"

echo "========================================="
echo "Expense Manager API Test Suite"
echo "========================================="
echo "Base URL: $BASE_URL"
echo "Test Email: $TEST_EMAIL"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ PASS${NC} - $2"
    else
        echo -e "${RED}✗ FAIL${NC} - $2"
    fi
}

# Function to extract JSON field using grep and sed (no jq dependency)
extract_json_field() {
    echo "$1" | grep -o "\"$2\":\"[^\"]*\"" | sed "s/\"$2\":\"\([^\"]*\)\"/\1/"
}

echo "========================================="
echo "1. Health Check"
echo "========================================="
HEALTH_RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/health")
HTTP_CODE=$(echo "$HEALTH_RESPONSE" | tail -n1)
BODY=$(echo "$HEALTH_RESPONSE" | head -n1)

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ] && [[ "$BODY" == *"ok"* ]]; then
    print_result 0 "Health endpoint"
else
    print_result 1 "Health endpoint"
    exit 1
fi
echo ""

echo "========================================="
echo "2. Register New User"
echo "========================================="
REGISTER_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

HTTP_CODE=$(echo "$REGISTER_RESPONSE" | tail -n1)
BODY=$(echo "$REGISTER_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    TOKEN=$(extract_json_field "$BODY" "token")
    print_result 0 "User registration"
    echo -e "${YELLOW}Token:${NC} ${TOKEN:0:50}..."
else
    print_result 1 "User registration"
    exit 1
fi
echo ""

echo "========================================="
echo "3. Login with Registered User"
echo "========================================="
LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n1)
BODY=$(echo "$LOGIN_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    TOKEN=$(extract_json_field "$BODY" "token")
    print_result 0 "User login"
    echo -e "${YELLOW}Token updated from login:${NC} ${TOKEN:0:50}..."
else
    print_result 1 "User login"
    exit 1
fi
echo ""

echo "========================================="
echo "4. Get Current User Info"
echo "========================================="
ME_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/auth/me" \
    -H "Authorization: Bearer $TOKEN")

HTTP_CODE=$(echo "$ME_RESPONSE" | tail -n1)
BODY=$(echo "$ME_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ] && [[ "$BODY" == *"$TEST_EMAIL"* ]]; then
    print_result 0 "Get current user"
else
    print_result 1 "Get current user"
fi
echo ""

echo "========================================="
echo "5. Create Test Expense"
echo "========================================="
CREATE_EXPENSE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/expenses" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{
        "businessName": "Test Restaurant",
        "businessId": "123456789",
        "invoiceNumber": "INV-001",
        "serviceDescription": "Lunch meeting",
        "transactionDate": "2025-12-20T12:00:00Z",
        "amountBeforeVat": 100.00,
        "amountAfterVat": 117.00,
        "vatAmount": 17.00,
        "category": "FOOD"
    }')

HTTP_CODE=$(echo "$CREATE_EXPENSE_RESPONSE" | tail -n1)
BODY=$(echo "$CREATE_EXPENSE_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    EXPENSE_ID=$(extract_json_field "$BODY" "id")
    print_result 0 "Create expense"
    echo -e "${YELLOW}Expense ID:${NC} $EXPENSE_ID"
else
    print_result 1 "Create expense"
    EXPENSE_ID=""
fi
echo ""

echo "========================================="
echo "6. Get All Expenses"
echo "========================================="
GET_EXPENSES_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/expenses" \
    -H "Authorization: Bearer $TOKEN")

HTTP_CODE=$(echo "$GET_EXPENSES_RESPONSE" | tail -n1)
BODY=$(echo "$GET_EXPENSES_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Get expenses"
else
    print_result 1 "Get expenses"
fi
echo ""

echo "========================================="
echo "7. Filter Expenses by Category"
echo "========================================="
FILTER_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/expenses?category=FOOD" \
    -H "Authorization: Bearer $TOKEN")

HTTP_CODE=$(echo "$FILTER_RESPONSE" | tail -n1)
BODY=$(echo "$FILTER_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Filter expenses by category"
else
    print_result 1 "Filter expenses by category"
fi
echo ""

if [ -n "$EXPENSE_ID" ]; then
    echo "========================================="
    echo "8. Update Expense Category"
    echo "========================================="
    UPDATE_RESPONSE=$(curl -s -w "\n%{http_code}" -X PATCH "$BASE_URL/api/expenses/$EXPENSE_ID" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $TOKEN" \
        -d '{"category": "OPERATIONS"}')

    HTTP_CODE=$(echo "$UPDATE_RESPONSE" | tail -n1)
    BODY=$(echo "$UPDATE_RESPONSE" | sed '$d')

    echo "Response: $BODY"
    echo "HTTP Code: $HTTP_CODE"

    if [ "$HTTP_CODE" = "200" ]; then
        print_result 0 "Update expense"
    else
        print_result 1 "Update expense"
    fi
    echo ""

    echo "========================================="
    echo "9. Delete Expense"
    echo "========================================="
    DELETE_RESPONSE=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL/api/expenses/$EXPENSE_ID" \
        -H "Authorization: Bearer $TOKEN")

    HTTP_CODE=$(echo "$DELETE_RESPONSE" | tail -n1)
    BODY=$(echo "$DELETE_RESPONSE" | sed '$d')

    echo "Response: $BODY"
    echo "HTTP Code: $HTTP_CODE"

    if [ "$HTTP_CODE" = "200" ]; then
        print_result 0 "Delete expense"
    else
        print_result 1 "Delete expense"
    fi
    echo ""
fi

echo "========================================="
echo "10. Test Invalid Login (Wrong Password)"
echo "========================================="
INVALID_LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"WrongPassword123\"}")

HTTP_CODE=$(echo "$INVALID_LOGIN_RESPONSE" | tail -n1)
BODY=$(echo "$INVALID_LOGIN_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "401" ]; then
    print_result 0 "Invalid login rejected"
else
    print_result 1 "Invalid login should return 401"
fi
echo ""

echo "========================================="
echo "11. Test Unauthorized Access"
echo "========================================="
UNAUTH_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/expenses")

HTTP_CODE=$(echo "$UNAUTH_RESPONSE" | tail -n1)
BODY=$(echo "$UNAUTH_RESPONSE" | sed '$d')

echo "Response: $BODY"
echo "HTTP Code: $HTTP_CODE"

if [ "$HTTP_CODE" = "401" ]; then
    print_result 0 "Unauthorized access blocked"
else
    print_result 1 "Unauthorized access should return 401"
fi
echo ""

echo "========================================="
echo "Test Suite Complete!"
echo "========================================="
echo ""
echo "Summary:"
echo "- API is deployed at: $BASE_URL"
echo "- Test user created: $TEST_EMAIL"
echo "- All core endpoints tested"
echo ""
echo "Next steps:"
echo "1. Test document upload with actual PDF/image files"
echo "2. Test OCR analysis endpoint"
echo "3. Build frontend to consume this API"
