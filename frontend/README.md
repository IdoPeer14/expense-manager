# Expense Manager Frontend

Modern React-based single-page application for expense management with intelligent document processing.

## Overview

The frontend is built with React 19 and Vite, providing a fast and responsive user experience. It features a clean, modern UI with full internationalization support (English and Hebrew) and automatic invoice data extraction.

## Tech Stack

- **React 19.2.0** - UI library with latest features
- **Vite 7.2.4** - Build tool and dev server
- **Tailwind CSS 3.4.19** - Utility-first CSS framework
- **React Query (TanStack Query)** - Server state management
- **React Hook Form** - Form validation and management
- **i18next** - Internationalization (English/Hebrew with RTL support)
- **Axios** - HTTP client for API communication
- **React PDF** - PDF viewing and rendering
- **React Dropzone** - Drag-and-drop file uploads

## Project Structure

```
frontend/
├── src/
│   ├── api/                    # API client and endpoint functions
│   │   ├── axiosConfig.js     # Axios instance with interceptors
│   │   └── endpoints.js       # API endpoint functions
│   │
│   ├── components/            # React components
│   │   ├── auth/             # Authentication components
│   │   │   ├── LoginForm.jsx
│   │   │   └── RegisterForm.jsx
│   │   ├── expenses/         # Expense management
│   │   │   ├── ExpenseForm.jsx
│   │   │   ├── ExpenseList.jsx
│   │   │   └── ExpenseItem.jsx
│   │   ├── upload/           # Document upload
│   │   │   ├── UploadArea.jsx
│   │   │   └── DocumentList.jsx
│   │   └── common/           # Shared components
│   │       ├── LanguageSwitcher.jsx
│   │       └── LoadingSpinner.jsx
│   │
│   ├── pages/                # Page components
│   │   ├── LoginPage.jsx
│   │   ├── DashboardPage.jsx
│   │   ├── UploadTab.jsx
│   │   └── ExpensesTab.jsx
│   │
│   ├── contexts/             # React context
│   │   └── AuthContext.jsx   # Authentication state
│   │
│   ├── hooks/                # Custom hooks
│   │   ├── useAuth.js       # Authentication hook
│   │   ├── useDocuments.js  # Document management
│   │   └── useExpenses.js   # Expense operations
│   │
│   ├── i18n/                 # Internationalization
│   │   ├── config.js        # i18next configuration
│   │   └── translations/    # Language files
│   │       ├── en.json
│   │       └── he.json
│   │
│   ├── utils/               # Utility functions
│   │   └── constants.js     # App constants
│   │
│   ├── App.jsx              # Root component
│   ├── App.css              # Global styles
│   ├── main.jsx             # Application entry point
│   └── index.css            # Tailwind imports
│
├── public/                  # Static assets
├── .env.example            # Environment template
├── .env                    # Environment variables (gitignored)
├── vite.config.js          # Vite configuration
├── tailwind.config.js      # Tailwind customization
├── postcss.config.js       # PostCSS configuration
├── package.json            # Dependencies and scripts
└── README.md               # This file
```

## Getting Started

### Prerequisites

- Node.js 18 or higher
- npm or yarn

### Installation

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Configure environment**
   ```bash
   cp .env.example .env
   # Edit .env and set VITE_API_URL to your backend URL
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```
   The app will be available at `http://localhost:5173`

### Build for Production

```bash
npm run build
```

The production build will be created in the `dist/` folder.

## Available Scripts

- `npm run dev` - Start development server with hot reload
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint

## Key Features

### Authentication
- User registration and login
- JWT token-based authentication
- Automatic token refresh
- Protected routes

### Document Upload
- Drag-and-drop file upload
- Support for PDF and image files (PNG, JPG, JPEG)
- Automatic OCR processing
- Document preview
- Processing status tracking

### Expense Management
- Create expenses manually or from uploaded documents
- Edit and delete expenses
- View expenses in a responsive table
- Filter and search capabilities
- Multi-currency support
- Categorization

### Internationalization
- Full support for English and Hebrew
- RTL (Right-to-Left) layout for Hebrew
- Language switcher component
- All UI text translated

See [README_I18N.md](README_I18N.md) for detailed i18n implementation.

## Environment Variables

Create a `.env` file in the frontend directory:

```env
VITE_API_URL=http://localhost:5219
```

For production, update this to your production API URL.

## API Integration

The app communicates with the backend API using Axios. The API client is configured in `src/api/axiosConfig.js` with:

- Automatic JWT token injection
- Response interceptors for error handling
- Request/response logging in development

### API Endpoints Used

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/documents/upload` - Upload document for OCR
- `GET /api/documents` - List user's documents
- `POST /api/expenses` - Create expense
- `GET /api/expenses` - List user's expenses
- `PUT /api/expenses/:id` - Update expense
- `DELETE /api/expenses/:id` - Delete expense

## State Management

The app uses multiple state management approaches:

- **React Context** - Global auth state (`AuthContext`)
- **React Query** - Server state caching and synchronization
- **React Hook Form** - Form state management
- **Local State** - Component-specific UI state

## Styling

### Tailwind CSS

The app uses Tailwind CSS with custom configuration:

```javascript
// tailwind.config.js
module.exports = {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      // Custom colors, spacing, etc.
    }
  }
}
```

### RTL Support

Hebrew language automatically enables RTL layout using the `dir="rtl"` attribute on the root element.

## Component Patterns

### Custom Hooks

All data fetching uses custom hooks that wrap React Query:

```javascript
// Example: useExpenses.js
export const useExpenses = () => {
  return useQuery({
    queryKey: ['expenses'],
    queryFn: api.getExpenses
  });
};
```

### Form Handling

Forms use React Hook Form for validation:

```javascript
const { register, handleSubmit, formState: { errors } } = useForm();
```

## Development Guidelines

### Code Style

- Use functional components with hooks
- Use arrow functions for component definitions
- Keep components small and focused
- Use descriptive variable names
- Extract repeated logic into custom hooks

### File Naming

- Components: PascalCase (e.g., `ExpenseForm.jsx`)
- Hooks: camelCase with `use` prefix (e.g., `useExpenses.js`)
- Utils: camelCase (e.g., `formatDate.js`)

### Component Organization

- Group related components in folders
- Keep page components in `/pages`
- Keep reusable components in `/components/common`
- Feature-specific components in `/components/{feature}`

## Troubleshooting

### Common Issues

**Port already in use**
```bash
# Kill process on port 5173
lsof -ti:5173 | xargs kill -9
```

**API connection errors**
- Check that backend is running
- Verify `VITE_API_URL` in `.env`
- Check browser console for CORS errors

**Build errors**
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

## Performance Optimization

- Route-based code splitting (coming soon)
- React Query caching reduces API calls
- Vite's fast HMR for development
- Optimized production builds with minification

## Browser Support

- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)

## Contributing

1. Follow the existing code style
2. Test your changes locally
3. Update i18n translations for new text
4. Ensure the build passes: `npm run build`

## Related Documentation

- [Main README](../README.md) - Project overview
- [Backend README](../backend/README.md) - API documentation
- [i18n Guide](README_I18N.md) - Internationalization details
- [Deployment Guide](../DEPLOYMENT.md) - Production deployment

## License

This project is proprietary software. All rights reserved.
