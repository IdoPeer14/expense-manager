מצוין.
להלן **PSK מלא כקובץ Markdown באנגלית**, מותאם בדיוק ל־**Tesseract OCR**, מוכן להדבקה כ־`README.md` או `SPEC.md` בריפו.

---

```md
# Expense & Invoice Management System — PSK (Full Specification)

## 1. Purpose
The goal of this project is to build a **secure full-stack web system** for managing business expenses using uploaded invoices/receipts.

The system supports:
- Secure user registration and authentication
- Uploading invoice/receipt files (PDF / images)
- Server-side OCR analysis using **Tesseract**
- Automatic extraction of invoice data
- Expense classification and management
- Server-side filtering and secure data access
- Full deployment using Docker and Render

The focus is **functionality, security, and architecture**, not UI design.

---

## 2. Core Principles
- All sensitive operations are executed **server-side only**:
  - OCR processing
  - Database access
  - Filtering and queries
- No secrets or OCR keys exposed to the client
- RESTful API with clear separation of concerns
- Secure authentication (JWT)
- HTTPS in production
- Server-side filtering only (no client-side filtering)

---

## 3. Technology Stack

### Backend
- ASP.NET Core Web API (C#)
- RESTful architecture
- Dockerized deployment
- Hosted on Render

### Database
- PostgreSQL (Render managed database)

### OCR
- **Tesseract OCR**
- Server-side execution only
- No external paid services

### Authentication
- JWT (Bearer Token or HttpOnly Cookie)
- Password hashing (BCrypt / ASP.NET PasswordHasher)

### Frontend
- Any SPA (React / Angular / Next.js)
- Minimal UI (functional, not design-oriented)

---

## 4. High-Level Architecture

```

Client (SPA)
|
|  HTTPS / REST
|
API Server (ASP.NET Core)
|
|-- Authentication (JWT)
|-- File Upload
|-- OCR (Tesseract)
|-- Expense CRUD
|-- Server-side Filtering
|
PostgreSQL Database

```

---

## 5. Client Views (Minimal UX)

### 5.1 Authentication
- Register (email + password)
- Login
- Redirect to dashboard after login

### 5.2 Dashboard
Contains two tabs:

#### Tab 1 — Upload Invoice / Receipt
- Upload PDF / JPG / PNG
- Analyze document (OCR)
- Display extracted fields
- Allow manual correction
- Save as expense

#### Tab 2 — Expense Management
- Table of all expenses
- Editable category per row
- Filters:
  - Category
  - Date range
  - Amount range
- Delete expense

---

## 6. Database Schema

### 6.1 Users
```

Users

* Id (UUID, PK)
* Email (unique, indexed)
* PasswordHash
* CreatedAt

```

### 6.2 Documents
```

Documents

* Id (UUID, PK)
* UserId (FK -> Users.Id)
* OriginalFileName
* MimeType
* StoragePath
* UploadedAt
* OcrText (TEXT, nullable)
* ParsedJson (JSONB, nullable)
* ParseStatus (PENDING / SUCCESS / FAILED)
* ParseError (nullable)

```

### 6.3 Expenses
```

Expenses

* Id (UUID, PK)
* UserId (FK -> Users.Id)
* DocumentId (nullable FK -> Documents.Id)
* BusinessName
* BusinessId (VAT / Company ID, nullable)
* InvoiceNumber (nullable)
* ServiceDescription (nullable)
* TransactionDate (DATE)
* AmountBeforeVat (NUMERIC)
* AmountAfterVat (NUMERIC)
* VatAmount (NUMERIC)
* Category (ENUM or STRING)
* CreatedAt
* UpdatedAt

````

### Categories
- FOOD
- VEHICLE
- IT
- OPERATIONS
- TRAINING
- OTHER

---

## 7. OCR Flow (Tesseract)

### 7.1 Processing Flow
1. Client uploads file → `POST /api/documents`
2. Server saves file metadata
3. Client triggers analysis → `POST /api/documents/{id}/analyze`
4. Server:
   - Runs Tesseract OCR
   - Extracts raw text
   - Parses structured fields using regex & heuristics
5. Server returns extracted data
6. Client may correct data
7. Client saves expense → `POST /api/expenses`

### 7.2 Fields Extracted
- Business name
- Transaction date
- Amount before VAT
- Amount after VAT
- VAT amount
- Business ID (VAT / Company ID)
- Invoice number (if available)
- Service description (if available)

### 7.3 Parsing Strategy
- Regex-based extraction:
  - Date patterns
  - Monetary values
  - VAT keywords
  - Invoice number keywords
- Heuristic prioritization (largest amounts, known keywords)
- Manual correction allowed before persistence

> OCR accuracy is **not required to be perfect**.

---

## 8. REST API Design

### 8.1 Authentication

#### POST `/api/auth/register`
```json
{
  "email": "user@example.com",
  "password": "password"
}
````

#### POST `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "password"
}
```

Response:

* JWT token

#### GET `/api/auth/me`

Returns authenticated user info

---

### 8.2 Documents

#### POST `/api/documents` (Auth required)

* multipart/form-data (`file`)
  Response:

```json
{
  "documentId": "uuid",
  "status": "PENDING"
}
```

#### POST `/api/documents/{id}/analyze` (Auth required)

Response:

```json
{
  "businessName": "...",
  "transactionDate": "2025-01-01",
  "amountBeforeVat": 100,
  "amountAfterVat": 117,
  "vatAmount": 17,
  "businessId": "...",
  "invoiceNumber": "...",
  "serviceDescription": "..."
}
```

---

### 8.3 Expenses

#### POST `/api/expenses`

Creates a new expense

#### GET `/api/expenses`

Server-side filtering:

* category
* date range
* amount range

#### PATCH `/api/expenses/{id}`

Update editable fields (category, etc.)

#### DELETE `/api/expenses/{id}`

Delete expense

---

## 9. Security Requirements

* JWT authentication on all protected endpoints
* Password hashing (never store plaintext passwords)
* Authorization checks (user can only access own data)
* File upload validation:

  * Allowed MIME types only
  * File size limit
* Secrets stored in environment variables only
* OCR executed strictly on server side

---

## 10. Deployment (Render)

### Services

1. PostgreSQL (Render managed)
2. Web Service (Docker)

### Environment Variables

```
DATABASE_URL
JWT_SECRET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### Health Endpoint

```
GET /health
→ { "status": "ok" }
```

---

## 11. Deliverables

* Live deployed API
* GitHub repository
* README / PSK document
* Clear explanation of:

  * Architecture
  * Security
  * OCR flow
  * API endpoints

---

## 12. Development Roadmap

### Phase 1

* Project skeleton
* Docker + Render
* Database connection
* Health check

### Phase 2

* DB models + migrations

### Phase 3

* Authentication (JWT)

### Phase 4

* Expense CRUD + filters

### Phase 5

* OCR integration (Tesseract)

### Phase 6

* Minimal frontend

---

## 13. Notes

This system is a **technical demonstration**, not an accounting product.
The emphasis is on:

* Secure architecture
* Correct separation of responsibilities
* Server-side logic
* Clean REST API design

```
