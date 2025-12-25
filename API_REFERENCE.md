# API Reference

Complete API documentation for the Expense Manager backend.

**Base URL**: `http://localhost:5219` (development) or your production URL

**API Version**: 1.0

## Authentication

All endpoints except `/api/auth/*` require authentication using JWT Bearer tokens.

**Authorization Header Format:**
```
Authorization: Bearer {your_jwt_token}
```

---

## Authentication Endpoints

### Register User

Create a new user account.

**Endpoint:** `POST /api/auth/register`

**Authentication:** Not required

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Validation Rules:**
- `username`: Required, 3-50 characters, alphanumeric + underscore
- `email`: Required, valid email format, unique
- `password`: Required, minimum 6 characters

**Success Response:** `201 Created`
```json
{
  "userId": 1,
  "username": "john_doe",
  "email": "john@example.com"
}
```

**Error Responses:**

`400 Bad Request` - Validation errors
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

`409 Conflict` - Username or email already exists
```json
{
  "message": "Username or email already exists"
}
```

**Example:**
```bash
curl -X POST http://localhost:5219/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecurePass123!"
  }'
```

---

### Login

Authenticate and receive a JWT token.

**Endpoint:** `POST /api/auth/login`

**Authentication:** Not required

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Success Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "john_doe"
}
```

**Error Responses:**

`401 Unauthorized` - Invalid credentials
```json
{
  "message": "Invalid username or password"
}
```

**Example:**
```bash
curl -X POST http://localhost:5219/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "password": "SecurePass123!"
  }'
```

---

## Document Endpoints

### Upload Document

Upload a document (PDF or image) for OCR processing.

**Endpoint:** `POST /api/documents/upload`

**Authentication:** Required

**Request Body:** `multipart/form-data`
- `file`: The document file (PDF, PNG, JPG, JPEG)

**Success Response:** `201 Created`
```json
{
  "id": 123,
  "filename": "receipt.pdf",
  "filePath": "/uploads/documents/receipt_20251225_103045.pdf",
  "uploadedAt": "2025-12-25T10:30:45.123Z",
  "status": "Processed",
  "parsedData": {
    "businessName": "Acme Corporation",
    "amount": 150.50,
    "currency": "USD",
    "date": "2025-12-24T00:00:00Z",
    "invoiceNumber": "INV-2024-001",
    "businessId": "512345678",
    "referenceNumber": "REF-123456",
    "documentType": "Invoice"
  },
  "userId": 1
}
```

**Parsed Data Fields:**
- `businessName`: Extracted business/merchant name
- `amount`: Total amount from document
- `currency`: Currency code (USD, EUR, ILS, etc.)
- `date`: Document date
- `invoiceNumber`: Invoice or receipt number
- `businessId`: Business tax ID (Israeli format: 9 digits)
- `referenceNumber`: Transaction reference number
- `documentType`: Classification (Invoice, Receipt, etc.)

**Note:** Some fields may be `null` if not found in the document.

**Error Responses:**

`400 Bad Request` - Invalid file type or no file provided
```json
{
  "message": "Invalid file type. Only PDF, PNG, JPG, JPEG are allowed."
}
```

`500 Internal Server Error` - OCR processing failed
```json
{
  "message": "Failed to process document"
}
```

**Example:**
```bash
curl -X POST http://localhost:5219/api/documents/upload \
  -H "Authorization: Bearer {token}" \
  -F "file=@/path/to/receipt.pdf"
```

---

### Get All Documents

Retrieve all documents for the authenticated user.

**Endpoint:** `GET /api/documents`

**Authentication:** Required

**Query Parameters:** None

**Success Response:** `200 OK`
```json
[
  {
    "id": 123,
    "filename": "receipt.pdf",
    "filePath": "/uploads/documents/receipt_20251225_103045.pdf",
    "uploadedAt": "2025-12-25T10:30:45.123Z",
    "status": "Processed",
    "parsedData": {
      "businessName": "Acme Corporation",
      "amount": 150.50,
      "currency": "USD",
      "date": "2025-12-24T00:00:00Z",
      "invoiceNumber": "INV-2024-001",
      "businessId": "512345678",
      "referenceNumber": "REF-123456",
      "documentType": "Invoice"
    },
    "userId": 1
  },
  {
    "id": 124,
    "filename": "lunch_receipt.jpg",
    "uploadedAt": "2025-12-25T12:15:30.456Z",
    "status": "Failed",
    "parsedData": null,
    "userId": 1
  }
]
```

**Document Status Values:**
- `Pending`: Document uploaded, awaiting processing
- `Processing`: OCR extraction in progress
- `Processed`: Successfully processed and data extracted
- `Failed`: Processing failed

**Example:**
```bash
curl -X GET http://localhost:5219/api/documents \
  -H "Authorization: Bearer {token}"
```

---

### Get Document by ID

Retrieve a specific document.

**Endpoint:** `GET /api/documents/{id}`

**Authentication:** Required

**Path Parameters:**
- `id`: Document ID (integer)

**Success Response:** `200 OK`
```json
{
  "id": 123,
  "filename": "receipt.pdf",
  "filePath": "/uploads/documents/receipt_20251225_103045.pdf",
  "uploadedAt": "2025-12-25T10:30:45.123Z",
  "status": "Processed",
  "parsedData": {
    "businessName": "Acme Corporation",
    "amount": 150.50,
    "currency": "USD",
    "date": "2025-12-24T00:00:00Z",
    "invoiceNumber": "INV-2024-001",
    "businessId": "512345678",
    "referenceNumber": "REF-123456",
    "documentType": "Invoice"
  },
  "userId": 1
}
```

**Error Responses:**

`404 Not Found` - Document doesn't exist or belongs to another user
```json
{
  "message": "Document not found"
}
```

**Example:**
```bash
curl -X GET http://localhost:5219/api/documents/123 \
  -H "Authorization: Bearer {token}"
```

---

## Expense Endpoints

### Create Expense

Create a new expense entry.

**Endpoint:** `POST /api/expenses`

**Authentication:** Required

**Request Body:**
```json
{
  "description": "string",
  "amount": 0.00,
  "currency": "string",
  "date": "2025-12-25T00:00:00Z",
  "category": "string",
  "documentId": 123  // optional
}
```

**Field Descriptions:**
- `description`: Expense description (required, max 500 characters)
- `amount`: Expense amount (required, must be > 0)
- `currency`: Currency code (required, e.g., "USD", "EUR", "ILS")
- `date`: Expense date (required, ISO 8601 format)
- `category`: Expense category (required, e.g., "Office", "Travel", "Food")
- `documentId`: Optional link to a document (must be user's document)

**Success Response:** `201 Created`
```json
{
  "id": 456,
  "userId": 1,
  "documentId": 123,
  "description": "Office supplies - printer paper",
  "amount": 150.50,
  "currency": "USD",
  "date": "2025-12-24T00:00:00Z",
  "category": "Office",
  "createdAt": "2025-12-25T10:35:00.789Z",
  "updatedAt": "2025-12-25T10:35:00.789Z"
}
```

**Error Responses:**

`400 Bad Request` - Validation errors
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Amount": ["The Amount field must be greater than 0."],
    "Description": ["The Description field is required."]
  }
}
```

`404 Not Found` - Document ID doesn't exist or belongs to another user
```json
{
  "message": "Document not found"
}
```

**Example:**
```bash
curl -X POST http://localhost:5219/api/expenses \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Office supplies",
    "amount": 150.50,
    "currency": "USD",
    "date": "2025-12-24T00:00:00Z",
    "category": "Office",
    "documentId": 123
  }'
```

---

### Get All Expenses

Retrieve all expenses for the authenticated user.

**Endpoint:** `GET /api/expenses`

**Authentication:** Required

**Query Parameters:** None

**Success Response:** `200 OK`
```json
[
  {
    "id": 456,
    "userId": 1,
    "documentId": 123,
    "description": "Office supplies",
    "amount": 150.50,
    "currency": "USD",
    "date": "2025-12-24T00:00:00Z",
    "category": "Office",
    "createdAt": "2025-12-25T10:35:00.789Z",
    "updatedAt": "2025-12-25T10:35:00.789Z"
  },
  {
    "id": 457,
    "userId": 1,
    "documentId": null,
    "description": "Client lunch meeting",
    "amount": 85.00,
    "currency": "USD",
    "date": "2025-12-25T00:00:00Z",
    "category": "Food",
    "createdAt": "2025-12-25T12:20:00.456Z",
    "updatedAt": "2025-12-25T12:20:00.456Z"
  }
]
```

**Example:**
```bash
curl -X GET http://localhost:5219/api/expenses \
  -H "Authorization: Bearer {token}"
```

---

### Get Expense by ID

Retrieve a specific expense.

**Endpoint:** `GET /api/expenses/{id}`

**Authentication:** Required

**Path Parameters:**
- `id`: Expense ID (integer)

**Success Response:** `200 OK`
```json
{
  "id": 456,
  "userId": 1,
  "documentId": 123,
  "description": "Office supplies",
  "amount": 150.50,
  "currency": "USD",
  "date": "2025-12-24T00:00:00Z",
  "category": "Office",
  "createdAt": "2025-12-25T10:35:00.789Z",
  "updatedAt": "2025-12-25T10:35:00.789Z"
}
```

**Error Responses:**

`404 Not Found` - Expense doesn't exist or belongs to another user
```json
{
  "message": "Expense not found"
}
```

**Example:**
```bash
curl -X GET http://localhost:5219/api/expenses/456 \
  -H "Authorization: Bearer {token}"
```

---

### Update Expense

Update an existing expense.

**Endpoint:** `PUT /api/expenses/{id}`

**Authentication:** Required

**Path Parameters:**
- `id`: Expense ID (integer)

**Request Body:**
```json
{
  "description": "string",
  "amount": 0.00,
  "currency": "string",
  "date": "2025-12-25T00:00:00Z",
  "category": "string"
}
```

**Note:** All fields are required. You cannot update `documentId` or `userId`.

**Success Response:** `200 OK`
```json
{
  "id": 456,
  "userId": 1,
  "documentId": 123,
  "description": "Updated office supplies purchase",
  "amount": 175.00,
  "currency": "USD",
  "date": "2025-12-24T00:00:00Z",
  "category": "Office",
  "createdAt": "2025-12-25T10:35:00.789Z",
  "updatedAt": "2025-12-25T14:20:15.123Z"
}
```

**Error Responses:**

`400 Bad Request` - Validation errors
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Amount": ["The Amount field must be greater than 0."]
  }
}
```

`404 Not Found` - Expense doesn't exist or belongs to another user
```json
{
  "message": "Expense not found"
}
```

**Example:**
```bash
curl -X PUT http://localhost:5219/api/expenses/456 \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Updated office supplies",
    "amount": 175.00,
    "currency": "USD",
    "date": "2025-12-24T00:00:00Z",
    "category": "Office"
  }'
```

---

### Delete Expense

Delete an expense.

**Endpoint:** `DELETE /api/expenses/{id}`

**Authentication:** Required

**Path Parameters:**
- `id`: Expense ID (integer)

**Success Response:** `204 No Content`

No response body.

**Error Responses:**

`404 Not Found` - Expense doesn't exist or belongs to another user
```json
{
  "message": "Expense not found"
}
```

**Example:**
```bash
curl -X DELETE http://localhost:5219/api/expenses/456 \
  -H "Authorization: Bearer {token}"
```

---

## Error Handling

### Standard Error Response Format

All error responses follow this structure:

```json
{
  "type": "string",
  "title": "string",
  "status": 400,
  "errors": {
    "fieldName": ["error message"]
  }
}
```

or for non-validation errors:

```json
{
  "message": "Error description"
}
```

### HTTP Status Codes

| Status Code | Meaning |
|-------------|---------|
| 200 OK | Request successful (GET, PUT) |
| 201 Created | Resource created successfully (POST) |
| 204 No Content | Resource deleted successfully (DELETE) |
| 400 Bad Request | Validation error or malformed request |
| 401 Unauthorized | Missing or invalid JWT token |
| 403 Forbidden | Authenticated but not authorized |
| 404 Not Found | Resource not found |
| 409 Conflict | Resource conflict (e.g., duplicate username) |
| 500 Internal Server Error | Server error |

---

## Data Types and Formats

### Date/Time Format

All dates use **ISO 8601** format:
```
2025-12-25T10:30:45.123Z
```

### Currency Codes

Use standard 3-letter currency codes:
- `USD` - US Dollar
- `EUR` - Euro
- `ILS` - Israeli Shekel
- `GBP` - British Pound
- etc.

### Decimal Numbers

Amounts should be decimal numbers with up to 2 decimal places:
```json
{
  "amount": 150.50
}
```

---

## Rate Limiting

Currently, no rate limiting is implemented. This may be added in future versions.

---

## API Versioning

The current API is version 1.0. Future versions may be introduced with URL versioning:
```
/api/v2/expenses
```

---

## Interactive Documentation

When running the backend, access interactive API documentation at:

**Swagger UI:** `http://localhost:5219/swagger`

This provides:
- Interactive endpoint testing
- Request/response examples
- Schema definitions
- Authentication testing

---

## Common Workflows

### Complete User Registration and Expense Creation Flow

1. **Register a new user**
   ```bash
   POST /api/auth/register
   ```

2. **Login to get token**
   ```bash
   POST /api/auth/login
   # Save the returned token
   ```

3. **Upload a receipt**
   ```bash
   POST /api/documents/upload
   # Save the returned document ID
   ```

4. **Create expense from document**
   ```bash
   POST /api/expenses
   # Include the document ID from step 3
   ```

5. **View all expenses**
   ```bash
   GET /api/expenses
   ```

### Update Workflow

1. **Get current expense**
   ```bash
   GET /api/expenses/{id}
   ```

2. **Update expense**
   ```bash
   PUT /api/expenses/{id}
   # Include all required fields, even unchanged ones
   ```

---

## Related Documentation

- [README.md](README.md) - Project overview
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
- [Backend README](backend/README.md) - Backend implementation details
- [Frontend README](frontend/README.md) - Frontend integration guide
- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment instructions

---

## Support

For API issues or questions:
1. Check the Swagger documentation at `/swagger`
2. Review the backend logs
3. Contact the development team

## License

This API is proprietary software. All rights reserved.
