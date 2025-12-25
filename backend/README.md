# Expense Manager Backend

ASP.NET Core 8.0 Web API with intelligent invoice processing using OCR and automated data extraction.

## Overview

The backend provides a RESTful API for expense management with advanced OCR capabilities. It uses Tesseract OCR to extract structured data from receipts and invoices, making expense entry effortless.

## Tech Stack

- **.NET 8.0** - Latest LTS version of ASP.NET Core
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Primary database
- **Tesseract OCR** - Optical character recognition engine
- **JWT Bearer Authentication** - Secure token-based auth
- **Swagger/OpenAPI** - API documentation

## Project Structure

```
backend/
├── ExpenseManager.Api/           # Main API project
│   ├── Controllers/             # API endpoints
│   │   ├── AuthController.cs           # Authentication/registration
│   │   ├── DocumentsController.cs      # Document upload & OCR
│   │   ├── ExpensesController.cs       # Expense CRUD
│   │   └── BaseAuthController.cs       # Base auth functionality
│   │
│   ├── Models/                  # Database entities
│   │   ├── User.cs                     # User accounts
│   │   ├── Document.cs                 # Uploaded documents
│   │   ├── Expense.cs                  # Expense records
│   │   ├── ParsedInvoiceData.cs        # Extracted invoice data
│   │   ├── ExpenseCategory.cs          # Expense categories
│   │   └── DocumentParseStatus.cs      # Processing status enum
│   │
│   ├── Services/                # Business logic
│   │   ├── OcrService.cs               # Tesseract OCR with PDF caching
│   │   ├── InvoiceParser.cs            # Main extraction orchestrator
│   │   ├── JwtService.cs               # JWT token generation
│   │   ├── PasswordHasherService.cs    # Password hashing
│   │   ├── FileStorageService.cs       # File management
│   │   │
│   │   ├── Extractors/                 # Field extraction services
│   │   │   ├── DateExtractor.cs
│   │   │   ├── AmountExtractor.cs
│   │   │   ├── BusinessNameExtractor.cs
│   │   │   ├── InvoiceNumberExtractor.cs
│   │   │   ├── BusinessIdExtractor.cs
│   │   │   ├── ReferenceNumberExtractor.cs
│   │   │   └── DocumentTypeExtractor.cs
│   │   │
│   │   ├── Validators/                 # Data validation
│   │   │   └── BusinessIdValidator.cs
│   │   │
│   │   ├── Normalizers/                # Text normalization
│   │   │   └── TextNormalizer.cs
│   │   │
│   │   └── Fallbacks/                  # Fallback strategies
│   │       └── ...
│   │
│   ├── Data/                    # Database context
│   │   └── ApplicationDbContext.cs     # EF Core DbContext
│   │
│   ├── wwwroot/                 # Static files (frontend build)
│   ├── Program.cs               # App startup & configuration
│   ├── appsettings.json        # Production configuration
│   └── appsettings.Development.json  # Development config
│
├── ExtractionTester/            # Invoice extraction testing tool
└── OcrTest/                     # OCR functionality testing
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+
- Tesseract OCR (installed automatically via NuGet)

### Installation

1. **Clone and navigate to backend**
   ```bash
   cd backend/ExpenseManager.Api
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database**

   Update `appsettings.Development.json` with your PostgreSQL connection:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=expense_manager;Username=postgres;Password=yourpassword"
     }
   }
   ```

4. **Configure JWT**

   Update JWT settings in `appsettings.Development.json`:
   ```json
   {
     "Jwt": {
       "Key": "your-secret-key-at-least-32-characters-long",
       "Issuer": "ExpenseManagerApi",
       "Audience": "ExpenseManagerClient"
     }
   }
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   The API will be available at:
   - HTTP: `http://localhost:5219`
   - Swagger UI: `http://localhost:5219/swagger`

### Database Setup

The application automatically creates the database schema on startup using `EnsureCreated()`. For production, use migrations:

```bash
# Create a migration
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

## API Endpoints

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "username": "john_doe"
}
```

### Documents

#### Upload Document
```http
POST /api/documents/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [PDF or image file]
```

**Response:**
```json
{
  "id": 123,
  "filename": "receipt.pdf",
  "uploadedAt": "2025-12-25T10:30:00Z",
  "status": "Processed",
  "parsedData": {
    "businessName": "Acme Corp",
    "amount": 150.00,
    "date": "2025-12-24",
    "invoiceNumber": "INV-2024-001",
    "businessId": "512345678"
  }
}
```

#### Get Documents
```http
GET /api/documents
Authorization: Bearer {token}
```

### Expenses

#### Create Expense
```http
POST /api/expenses
Authorization: Bearer {token}
Content-Type: application/json

{
  "description": "Office supplies",
  "amount": 150.00,
  "currency": "USD",
  "date": "2025-12-24",
  "category": "Office",
  "documentId": 123
}
```

#### Get Expenses
```http
GET /api/expenses
Authorization: Bearer {token}
```

#### Update Expense
```http
PUT /api/expenses/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "description": "Updated description",
  "amount": 175.00,
  "currency": "USD",
  "date": "2025-12-24",
  "category": "Office"
}
```

#### Delete Expense
```http
DELETE /api/expenses/{id}
Authorization: Bearer {token}
```

See [API_REFERENCE.md](../API_REFERENCE.md) for complete API documentation.

## OCR and Invoice Extraction

### How It Works

1. **Document Upload** - User uploads PDF or image
2. **OCR Processing** - Tesseract extracts text from document
3. **Field Extraction** - Specialized extractors parse structured data:
   - Date extraction with multiple format support
   - Amount extraction with currency handling
   - Business name identification
   - Invoice/reference number extraction
   - Business ID validation (Israeli format)
4. **Data Validation** - Validators ensure data quality
5. **Result Caching** - PDF conversion results cached for performance

### Performance Optimizations

The OCR system includes several performance enhancements:

- **PDF Caching** - Converted PDFs cached to avoid re-conversion (30-50x faster)
- **Optimized PDF Conversion** - Improved conversion algorithms (2-3x faster)
- **Parallel Processing** - Multiple extractors run concurrently
- **Result Caching** - Extraction results cached per document

See [PERFORMANCE_IMPROVEMENTS.md](PERFORMANCE_IMPROVEMENTS.md) for detailed metrics.

### Supported Document Types

- **PDF** - Multi-page support with caching
- **Images** - PNG, JPG, JPEG

### Extraction Fields

- Business Name
- Amount (with currency)
- Date (multiple formats)
- Invoice Number
- Business ID (Israeli format with validation)
- Reference Number
- Document Type

## Services Architecture

### Core Services

#### OcrService
- Tesseract OCR integration
- PDF to image conversion
- Text extraction with caching
- Multi-page PDF support

#### InvoiceParser
- Orchestrates field extraction
- Manages extractor pipeline
- Applies validation rules
- Returns structured data

#### JwtService
- JWT token generation
- Token validation
- Claims management

#### PasswordHasherService
- Secure password hashing using BCrypt
- Password verification

#### FileStorageService
- File upload handling
- Storage management
- File retrieval

### Extractor Services

Each extractor focuses on a specific field:

- **DateExtractor** - Extracts dates with format normalization
- **AmountExtractor** - Finds monetary amounts with currency
- **BusinessNameExtractor** - Identifies business names
- **InvoiceNumberExtractor** - Extracts invoice numbers
- **BusinessIdExtractor** - Finds and validates business IDs
- **ReferenceNumberExtractor** - Extracts reference numbers
- **DocumentTypeExtractor** - Classifies document type

See [REGEX_EXTRACTION_SPECIFICATION.md](REGEX_EXTRACTION_SPECIFICATION.md) for technical details.

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=expense_manager;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "ExpenseManagerApi",
    "Audience": "ExpenseManagerClient"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables

For production, use environment variables:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Database=expense_manager;..."
export Jwt__Key="production-secret-key"
```

## Security

### Authentication
- JWT Bearer token authentication
- Secure password hashing with BCrypt
- Token expiration (configurable)

### Authorization
- All endpoints except `/api/auth/*` require authentication
- User-scoped data access (users can only access their own data)

### CORS
Configured for:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:3000` (Alternative frontend)
- Production domain (configure in `Program.cs`)

### Input Validation
- Model validation using Data Annotations
- Business ID format validation
- File type restrictions

## Testing

### Unit Testing Projects

- **ExtractionTester** - Test invoice extraction logic
- **OcrTest** - Test OCR functionality

### Running Tests

```bash
# Run extraction tester
cd backend/ExtractionTester
dotnet run

# Run OCR tests
cd backend/OcrTest
dotnet run
```

## Database Schema

### Tables

#### Users
- `Id` (Primary Key)
- `Username` (Unique)
- `Email` (Unique)
- `PasswordHash`
- `CreatedAt`

#### Documents
- `Id` (Primary Key)
- `UserId` (Foreign Key)
- `Filename`
- `FilePath`
- `UploadedAt`
- `Status` (Pending/Processing/Processed/Failed)
- `ParsedData` (JSON)

#### Expenses
- `Id` (Primary Key)
- `UserId` (Foreign Key)
- `DocumentId` (Foreign Key, nullable)
- `Description`
- `Amount`
- `Currency`
- `Date`
- `Category`
- `CreatedAt`
- `UpdatedAt`

## Deployment

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

### Running in Production

```bash
cd publish
dotnet ExpenseManager.Api.dll
```

### Serving Frontend

The backend serves the React frontend from `wwwroot/`. Use the build script:

```bash
# From project root
./build-and-deploy.sh
```

This:
1. Builds frontend with Vite
2. Copies build to `backend/ExpenseManager.Api/wwwroot/`
3. Builds backend in Release mode

See [DEPLOYMENT.md](../DEPLOYMENT.md) for detailed deployment instructions.

## Troubleshooting

### Common Issues

**Database connection errors**
- Verify PostgreSQL is running
- Check connection string in appsettings
- Ensure database exists

**OCR not working**
- Tesseract is installed via NuGet automatically
- Check file permissions for uploaded documents
- Verify supported file format (PDF, PNG, JPG, JPEG)

**JWT errors**
- Ensure JWT Key is at least 32 characters
- Check token expiration
- Verify Authorization header format: `Bearer {token}`

**CORS errors**
- Add frontend URL to CORS policy in `Program.cs`
- Ensure URL matches exactly (including port)

## Development Guidelines

### Code Style
- Use async/await for I/O operations
- Follow C# naming conventions
- Use dependency injection
- Keep controllers thin, logic in services

### Adding New Extractors
1. Create class in `Services/Extractors/`
2. Implement extraction logic
3. Add to `InvoiceParser` pipeline
4. Write tests in `ExtractionTester`

### Database Changes
1. Update models in `Models/`
2. Create migration: `dotnet ef migrations add MigrationName`
3. Review migration code
4. Apply: `dotnet ef database update`

## Related Documentation

- [Main README](../README.md) - Project overview
- [Frontend README](../frontend/README.md) - React app documentation
- [API Reference](../API_REFERENCE.md) - Complete API docs
- [Architecture](../ARCHITECTURE.md) - System design
- [Performance Improvements](PERFORMANCE_IMPROVEMENTS.md) - Optimization details
- [Regex Specification](REGEX_EXTRACTION_SPECIFICATION.md) - Extraction patterns
- [Deployment Guide](../DEPLOYMENT.md) - Production deployment

## License

This project is proprietary software. All rights reserved.
