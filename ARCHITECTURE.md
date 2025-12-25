# Expense Manager - System Architecture

## Overview

Expense Manager is a full-stack web application that combines intelligent document processing with expense tracking. The system uses OCR technology to automatically extract structured data from receipts and invoices, streamlining expense management.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Client Layer                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           React SPA (Vite Build)                     │   │
│  │  - Authentication UI                                  │   │
│  │  - Document Upload Interface                          │   │
│  │  - Expense Management Dashboard                       │   │
│  │  - i18n Support (English/Hebrew)                      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTPS / REST API
                            │
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │          ASP.NET Core 8.0 Web API                    │   │
│  │                                                        │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────┐ │   │
│  │  │ Auth         │  │ Documents    │  │ Expenses   │ │   │
│  │  │ Controller   │  │ Controller   │  │ Controller │ │   │
│  │  └──────────────┘  └──────────────┘  └────────────┘ │   │
│  │         │                  │                 │        │   │
│  │  ┌──────────────────────────────────────────────┐   │   │
│  │  │            Service Layer                     │   │   │
│  │  │  - JWT Authentication                         │   │   │
│  │  │  - OCR Processing (Tesseract)                │   │   │
│  │  │  - Invoice Data Extraction                    │   │   │
│  │  │  - File Storage                               │   │   │
│  │  │  - Business Logic                             │   │   │
│  │  └──────────────────────────────────────────────┘   │   │
│  │         │                                            │   │
│  │  ┌──────────────────────────────────────────────┐   │   │
│  │  │        Data Access Layer (EF Core)           │   │   │
│  │  └──────────────────────────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Entity Framework Core
                            │
┌─────────────────────────────────────────────────────────────┐
│                       Data Layer                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              PostgreSQL Database                      │   │
│  │  - Users                                              │   │
│  │  - Documents                                          │   │
│  │  - Expenses                                           │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    External Services                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Tesseract OCR Engine                          │   │
│  │  - Text extraction from images/PDFs                   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Application Flow

### 1. User Authentication Flow

```
User → Login Form → POST /api/auth/login
                          ↓
                    Verify Credentials
                          ↓
                    Generate JWT Token
                          ↓
                    Return Token to Client
                          ↓
                    Store Token (localStorage)
                          ↓
            Token sent in Authorization header
            for all subsequent requests
```

### 2. Document Upload and Processing Flow

```
User → Upload Document → POST /api/documents/upload
                              ↓
                        Save File to Disk
                              ↓
                        Create Document Record
                              ↓
                        ┌─────────────────┐
                        │  OCR Pipeline   │
                        └─────────────────┘
                              ↓
            ┌─────────────────┴─────────────────┐
            │                                   │
      PDF Document?                       Image Document
            │                                   │
            ├─ Convert to Images                │
            │  (with caching)                   │
            └─────────────────┬─────────────────┘
                              ↓
                        Tesseract OCR
                        Extract Text
                              ↓
                        ┌───────────────────┐
                        │ Field Extraction  │
                        └───────────────────┘
                              ↓
            ┌─────────────────┼─────────────────┐
            │                 │                 │
      Date Extractor   Amount Extractor   Business Name
            │                 │                 │
    Invoice Number      Business ID      Reference Number
            │                 │                 │
            └─────────────────┼─────────────────┘
                              ↓
                        Validate Data
                              ↓
                  Store ParsedInvoiceData
                              ↓
                  Update Document Status
                              ↓
                  Return Parsed Data to Client
```

### 3. Expense Management Flow

```
User → Create Expense → POST /api/expenses
                            ↓
                    Validate Input
                            ↓
                Link to Document (if provided)
                            ↓
                Save to Database
                            ↓
              Return Created Expense
                            ↓
            Update UI (React Query)
```

## Technology Stack

### Frontend Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Framework** | React 19.2.0 | UI library |
| **Build Tool** | Vite 7.2.4 | Fast dev server & bundling |
| **Styling** | Tailwind CSS 3.4.19 | Utility-first CSS |
| **State Management** | React Query + Context | Server & client state |
| **Forms** | React Hook Form | Form validation |
| **HTTP Client** | Axios | API communication |
| **i18n** | i18next | Internationalization |
| **PDF Rendering** | React PDF | Document preview |
| **File Upload** | React Dropzone | Drag-and-drop uploads |

### Backend Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core 8.0 | Web API framework |
| **ORM** | Entity Framework Core | Database access |
| **Database** | PostgreSQL 12+ | Data persistence |
| **Authentication** | JWT Bearer | Token-based auth |
| **OCR Engine** | Tesseract 5.x | Text recognition |
| **Password Hashing** | BCrypt | Secure password storage |
| **API Docs** | Swagger/OpenAPI | Interactive API documentation |

## Data Models

### User
```csharp
class User {
  int Id;
  string Username;      // Unique
  string Email;         // Unique
  string PasswordHash;  // BCrypt hashed
  DateTime CreatedAt;
}
```

### Document
```csharp
class Document {
  int Id;
  int UserId;
  string Filename;
  string FilePath;
  DateTime UploadedAt;
  DocumentParseStatus Status;  // Pending/Processing/Processed/Failed
  ParsedInvoiceData ParsedData;
}
```

### ParsedInvoiceData
```csharp
class ParsedInvoiceData {
  string BusinessName;
  decimal? Amount;
  string Currency;
  DateTime? Date;
  string InvoiceNumber;
  string BusinessId;
  string ReferenceNumber;
  string DocumentType;
}
```

### Expense
```csharp
class Expense {
  int Id;
  int UserId;
  int? DocumentId;      // Optional link to document
  string Description;
  decimal Amount;
  string Currency;
  DateTime Date;
  string Category;
  DateTime CreatedAt;
  DateTime UpdatedAt;
}
```

## Security Architecture

### Authentication

1. **User Registration**
   - Password hashed with BCrypt (cost factor 12)
   - Username and email uniqueness enforced
   - Validation on all inputs

2. **User Login**
   - Credentials verified against hashed password
   - JWT token generated with claims:
     - `sub`: User ID
     - `unique_name`: Username
     - `exp`: Expiration timestamp
   - Token signed with HMAC-SHA256

3. **Token Usage**
   - Client stores token in localStorage
   - Sent in `Authorization: Bearer {token}` header
   - Backend validates signature and expiration
   - User ID extracted from claims

### Authorization

- **Public Endpoints**: `/api/auth/login`, `/api/auth/register`
- **Protected Endpoints**: All others require valid JWT
- **Data Scoping**: Users can only access their own data
  - Enforced by filtering queries by `UserId`
  - Controllers use `User.FindFirst("sub")` to get user ID

### CORS Policy

Configured to allow requests from:
- `http://localhost:5173` - Vite dev server
- `http://localhost:3000` - Alternative frontend
- Production domain (configured in deployment)

### Input Validation

- Model validation using Data Annotations
- File type restrictions (PDF, PNG, JPG, JPEG)
- File size limits
- Business ID format validation (Israeli format)

## OCR and Extraction Architecture

### OCR Processing

```
Document Upload
    ↓
┌──────────────────┐
│  OcrService      │
├──────────────────┤
│ - PDF Detection  │
│ - Image Convert  │
│ - Text Extract   │
│ - Caching        │
└──────────────────┘
    ↓
Raw Text Output
    ↓
┌──────────────────┐
│ InvoiceParser    │
├──────────────────┤
│ - Orchestration  │
│ - Field Extract  │
│ - Validation     │
│ - Normalization  │
└──────────────────┘
    ↓
Structured Data
```

### Extraction Pipeline

The system uses specialized extractors that run in parallel:

1. **DateExtractor**
   - Regex patterns for common date formats
   - Format normalization to ISO 8601
   - Fuzzy date recognition

2. **AmountExtractor**
   - Decimal number detection
   - Currency symbol recognition
   - Total amount identification

3. **BusinessNameExtractor**
   - Header text extraction
   - Business name patterns
   - Confidence scoring

4. **InvoiceNumberExtractor**
   - Invoice/receipt number patterns
   - Multiple format support

5. **BusinessIdExtractor**
   - Israeli business ID format (9 digits)
   - Checksum validation
   - OCR error correction

6. **ReferenceNumberExtractor**
   - Transaction/reference number patterns

7. **DocumentTypeExtractor**
   - Document classification (invoice, receipt, etc.)

### Performance Optimizations

1. **PDF Caching**
   - Converted PDFs cached to avoid re-conversion
   - 30-50x faster on cache hits
   - Cache invalidation on file changes

2. **Optimized PDF Conversion**
   - Improved algorithms for PDF to image
   - 2-3x faster than original implementation
   - Better quality output

3. **Parallel Extraction**
   - Field extractors run concurrently
   - Reduced total processing time

4. **Result Caching**
   - Parsed results stored in database
   - Avoid re-processing same document

## Deployment Architecture

### Development Environment

```
Frontend:  http://localhost:5173 (Vite dev server)
Backend:   http://localhost:5219 (Kestrel)
Database:  localhost:5432 (PostgreSQL)
```

### Production Environment (Render.com)

```
┌─────────────────────────────────────────┐
│         Render Web Service              │
│  ┌───────────────────────────────────┐  │
│  │   .NET Runtime                    │  │
│  │   ├─ Serves API (/api/*)          │  │
│  │   └─ Serves Static Files (SPA)    │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      Render PostgreSQL Service          │
│  - Managed PostgreSQL instance          │
│  - Automatic backups                    │
└─────────────────────────────────────────┘
```

### Build Process

1. **Frontend Build**
   ```bash
   cd frontend
   npm run build  # Creates frontend/dist/
   ```

2. **Copy to Backend**
   ```bash
   cp -r frontend/dist/* backend/ExpenseManager.Api/wwwroot/
   ```

3. **Backend Build**
   ```bash
   cd backend/ExpenseManager.Api
   dotnet publish -c Release
   ```

4. **Deployment**
   - Push to Git repository
   - Render automatically builds and deploys
   - Static files served from wwwroot
   - SPA routing handled by fallback route

## API Design

### RESTful Principles

- Resource-based URLs (`/api/expenses`, `/api/documents`)
- HTTP verbs for actions (GET, POST, PUT, DELETE)
- Stateless requests (JWT in header)
- JSON request/response bodies
- Standard HTTP status codes

### Status Codes

| Code | Usage |
|------|-------|
| 200 OK | Successful GET, PUT |
| 201 Created | Successful POST |
| 204 No Content | Successful DELETE |
| 400 Bad Request | Validation errors |
| 401 Unauthorized | Missing/invalid token |
| 404 Not Found | Resource not found |
| 500 Internal Server Error | Server errors |

### Response Format

**Success Response:**
```json
{
  "id": 123,
  "description": "Office supplies",
  "amount": 150.00,
  "currency": "USD",
  "date": "2025-12-24",
  "category": "Office"
}
```

**Error Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Amount": ["The Amount field is required."]
  }
}
```

## Internationalization

### Frontend i18n

- **Library**: i18next with react-i18next
- **Languages**: English (en), Hebrew (he)
- **Features**:
  - Language switcher component
  - RTL support for Hebrew
  - Translation files: `src/i18n/translations/{lang}.json`
  - Automatic direction switching

### Implementation

```javascript
// Language detection
i18n.use(LanguageDetector).init({
  fallbackLng: 'en',
  supportedLngs: ['en', 'he']
});

// Usage in components
const { t } = useTranslation();
<button>{t('login.submit')}</button>
```

## Monitoring and Logging

### Backend Logging

- ASP.NET Core built-in logging
- Log levels: Debug, Information, Warning, Error, Critical
- Console output in development
- Production: Configure external logging service

### Error Handling

- Global exception handling middleware
- Structured error responses
- Database errors logged and sanitized
- OCR errors captured with context

## Scalability Considerations

### Current Architecture

- Monolithic application (frontend + backend)
- Single server deployment
- Direct file storage on disk

### Future Scalability

**Potential improvements:**

1. **Separate Frontend and Backend**
   - Deploy frontend to CDN
   - Scale backend independently

2. **Database Optimization**
   - Add indexes for common queries
   - Implement read replicas
   - Consider caching layer (Redis)

3. **File Storage**
   - Move to cloud storage (S3, Azure Blob)
   - Enable CDN for document serving

4. **OCR Processing**
   - Background job processing (Hangfire)
   - Queue-based architecture (RabbitMQ)
   - Separate OCR microservice

5. **Caching**
   - Redis for session management
   - Cache frequently accessed data
   - API response caching

## Testing Strategy

### Current Testing

- **ExtractionTester**: Manual testing of invoice extraction
- **OcrTest**: OCR functionality verification

### Recommended Testing

1. **Unit Tests**
   - Service layer logic
   - Extractor algorithms
   - Validation rules

2. **Integration Tests**
   - API endpoints
   - Database operations
   - OCR pipeline

3. **End-to-End Tests**
   - User workflows
   - Document upload and processing
   - Expense management

## Related Documentation

- [README.md](README.md) - Project overview and quick start
- [API_REFERENCE.md](API_REFERENCE.md) - Complete API documentation
- [Frontend README](frontend/README.md) - React app details
- [Backend README](backend/README.md) - .NET API details
- [DEPLOYMENT.md](DEPLOYMENT.md) - Production deployment guide
- [Performance Improvements](backend/PERFORMANCE_IMPROVEMENTS.md) - Optimization details

## License

This project is proprietary software. All rights reserved.
