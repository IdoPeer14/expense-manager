# Expense Manager

A full-stack expense management application with intelligent invoice processing and OCR capabilities. Upload receipts, automatically extract expense data, and manage your expenses efficiently.

## Features

- **User Authentication** - Secure JWT-based authentication with user registration and login
- **Document Upload** - Support for PDF and image files (PNG, JPG, JPEG)
- **Intelligent OCR** - Automatic invoice data extraction using Tesseract OCR
- **Expense Management** - Create, view, edit, and delete expenses
- **Multi-Currency Support** - Handle expenses in different currencies
- **Internationalization** - Full support for English and Hebrew (RTL)
- **Performance Optimized** - PDF caching and optimized extraction algorithms
- **Responsive Design** - Modern UI built with React and Tailwind CSS

## Tech Stack

### Frontend
- **React 19** - Modern UI library
- **Vite** - Fast build tool and development server
- **Tailwind CSS** - Utility-first CSS framework
- **React Query** - Data fetching and caching
- **React Hook Form** - Form validation
- **i18next** - Internationalization framework
- **Axios** - HTTP client

### Backend
- **.NET 8.0** - ASP.NET Core Web API
- **Entity Framework Core** - ORM for database access
- **PostgreSQL** - Primary database
- **Tesseract OCR** - Optical character recognition
- **JWT Authentication** - Secure token-based auth
- **Swagger/OpenAPI** - API documentation

## Quick Start

### Prerequisites

- **Node.js** 18+ and npm
- **.NET 8.0 SDK**
- **PostgreSQL** 12+

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd expense-manager
   ```

2. **Set up the database**
   ```bash
   # Create a PostgreSQL database named 'expense_manager'
   createdb expense_manager
   ```

3. **Configure the backend**
   ```bash
   cd backend/ExpenseManager.Api

   # Update appsettings.Development.json with your database connection string
   # Example: "Host=localhost;Database=expense_manager;Username=postgres;Password=yourpassword"
   ```

4. **Configure the frontend**
   ```bash
   cd frontend

   # Copy the environment template
   cp .env.example .env

   # Update .env with your API URL (default: http://localhost:5219)
   ```

5. **Install dependencies and run**

   **Backend:**
   ```bash
   cd backend/ExpenseManager.Api
   dotnet restore
   dotnet run
   ```
   The API will be available at `http://localhost:5219`
   API documentation at `http://localhost:5219/swagger`

   **Frontend:**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   The app will be available at `http://localhost:5173`

## Project Structure

```
expense-manager/
├── frontend/              # React + Vite SPA
│   ├── src/
│   │   ├── components/   # React components organized by feature
│   │   ├── pages/        # Page components
│   │   ├── api/          # API client and endpoints
│   │   ├── hooks/        # Custom React hooks
│   │   ├── contexts/     # React context providers
│   │   ├── i18n/         # Internationalization
│   │   └── utils/        # Helper functions
│   └── README.md         # Frontend documentation
│
├── backend/              # .NET Core API
│   ├── ExpenseManager.Api/
│   │   ├── Controllers/  # API endpoints
│   │   ├── Models/       # Database entities
│   │   ├── Services/     # Business logic
│   │   └── Data/         # Database context
│   └── README.md         # Backend documentation
│
├── DEPLOYMENT.md         # Production deployment guide
├── ARCHITECTURE.md       # System design documentation
├── API_REFERENCE.md      # API endpoint reference
└── build-and-deploy.sh   # Build automation script
```

## Documentation

- **[Frontend Documentation](frontend/README.md)** - React app structure and components
- **[Backend Documentation](backend/README.md)** - API architecture and services
- **[API Reference](API_REFERENCE.md)** - Complete API endpoint documentation
- **[Architecture Overview](ARCHITECTURE.md)** - System design and data flow
- **[Deployment Guide](DEPLOYMENT.md)** - Production deployment instructions
- **[Internationalization](frontend/README_I18N.md)** - i18n implementation details
- **[Performance Improvements](backend/PERFORMANCE_IMPROVEMENTS.md)** - Optimization documentation

## Building for Production

Use the provided build script to create a production build:

```bash
./build-and-deploy.sh
```

This script:
1. Builds the frontend with Vite
2. Copies the built files to the backend's wwwroot folder
3. Builds the backend in Release mode

The backend serves the frontend as static files, making deployment simple.

## API Overview

The API provides the following main endpoints:

- **`/api/auth`** - User authentication and registration
- **`/api/documents`** - Document upload and OCR processing
- **`/api/expenses`** - Expense CRUD operations

For detailed endpoint documentation, see [API_REFERENCE.md](API_REFERENCE.md)

## Environment Variables

### Frontend (.env)
```
VITE_API_URL=http://localhost:5219
```

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=expense_manager;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "ExpenseManagerApi",
    "Audience": "ExpenseManagerClient"
  }
}
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is proprietary software. All rights reserved.

## Support

For issues or questions, please contact the development team.
