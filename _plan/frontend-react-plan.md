# React Frontend Implementation Plan
## Expense & Invoice Management System

---

## 1. Overview

This document outlines the implementation plan for the React-based frontend of the Expense & Invoice Management System. The UI follows the design patterns established in the reference HTML files and integrates with the ASP.NET Core backend API.

### Key Objectives
- Modern, functional SPA using React
- TailwindCSS for styling with dark mode support
- JWT-based authentication
- File upload with OCR preview
- Expense management with server-side filtering
- Responsive design (mobile-first)

---

## 2. Technology Stack

### Core
- **React** 18+ (with Hooks)
- **React Router** v6 (client-side routing)
- **TailwindCSS** (styling framework)
- **Vite** (build tool - fast, modern)

### State Management
- **React Context API** (auth state, theme)
- **React Query / TanStack Query** (server state, caching)
- Alternative: **Zustand** (if more complex state needed)

### Form Handling
- **React Hook Form** (form validation)
- **Zod** (schema validation)

### HTTP Client
- **Axios** (API calls with interceptors for JWT)

### UI Components
- **Headless UI** (accessible components)
- **Material Symbols** (icons - already in refs)

### File Upload
- **React Dropzone** (drag & drop file upload)

### Date Handling
- **date-fns** (lightweight date manipulation)

---

## 3. Project Structure

```
frontend/
├── public/
│   └── index.html
├── src/
│   ├── api/
│   │   ├── client.js          # Axios instance with interceptors
│   │   ├── auth.js            # Auth API calls
│   │   ├── documents.js       # Document/upload API calls
│   │   └── expenses.js        # Expense API calls
│   ├── components/
│   │   ├── auth/
│   │   │   ├── LoginForm.jsx
│   │   │   ├── RegisterForm.jsx
│   │   │   └── ProtectedRoute.jsx
│   │   ├── common/
│   │   │   ├── Button.jsx
│   │   │   ├── Input.jsx
│   │   │   ├── Select.jsx
│   │   │   ├── ErrorAlert.jsx
│   │   │   ├── LoadingSpinner.jsx
│   │   │   └── Header.jsx
│   │   ├── upload/
│   │   │   ├── DropZone.jsx
│   │   │   ├── DocumentPreview.jsx
│   │   │   ├── ExtractedDataForm.jsx
│   │   │   └── UploadStatus.jsx
│   │   └── expenses/
│   │       ├── ExpenseTable.jsx
│   │       ├── ExpenseTableRow.jsx
│   │       ├── ExpenseFilters.jsx
│   │       └── CategorySelect.jsx
│   ├── contexts/
│   │   ├── AuthContext.jsx    # User auth state
│   │   └── ThemeContext.jsx   # Dark mode toggle
│   ├── hooks/
│   │   ├── useAuth.js         # Auth helpers
│   │   ├── useDocuments.js    # React Query hooks for docs
│   │   ├── useExpenses.js     # React Query hooks for expenses
│   │   └── useTheme.js        # Dark mode hook
│   ├── pages/
│   │   ├── LoginPage.jsx
│   │   ├── RegisterPage.jsx
│   │   ├── DashboardPage.jsx  # Container for tabs
│   │   ├── UploadTab.jsx      # Upload invoices tab
│   │   └── ExpensesTab.jsx    # Expenses management tab
│   ├── utils/
│   │   ├── constants.js       # API URLs, categories
│   │   ├── validation.js      # Zod schemas
│   │   └── formatters.js      # Date/currency formatters
│   ├── App.jsx
│   ├── main.jsx
│   └── index.css              # Tailwind imports
├── tailwind.config.js
├── vite.config.js
└── package.json
```

---

## 4. Routing Structure

### Routes
```
/ (public)
├── /login          → LoginPage
├── /register       → RegisterPage
└── /dashboard (protected)
    ├── /dashboard/upload    → UploadTab
    └── /dashboard/expenses  → ExpensesTab
```

### Protected Route Logic
- Check JWT token in localStorage/cookie
- Redirect to `/login` if not authenticated
- Verify token with `/api/auth/me` on mount

---

## 5. Page Specifications

### 5.1 Login Page (`LoginPage.jsx`)

**Design Reference:** `login.html`

**Features:**
- Email/password form
- Client-side validation (email format, required fields)
- Error message display (invalid credentials)
- Loading state on submit
- "Forgot Password" link (placeholder)
- "Create account" link → `/register`
- Optional: Google OAuth button (future)

**State:**
- Form data (email, password)
- Loading state
- Error message

**API Integration:**
```javascript
POST /api/auth/login
Request: { email, password }
Response: { token, user }
```

**Actions:**
1. Submit form
2. Store JWT in localStorage
3. Redirect to `/dashboard/upload`

---

### 5.2 Register Page (`RegisterPage.jsx`)

**Design Reference:** `signup.html`

**Features:**
- Email/password/confirmPassword form
- Password strength indicator
- Terms & conditions checkbox
- Client-side validation:
  - Email format
  - Password min 8 characters
  - Passwords match
- Error display (email already exists)
- Loading state
- "Log in" link → `/login`

**State:**
- Form data
- Validation errors
- Loading state
- API error

**API Integration:**
```javascript
POST /api/auth/register
Request: { email, password }
Response: { token, user }
```

**Actions:**
1. Validate form
2. Submit registration
3. Auto-login on success
4. Redirect to `/dashboard/upload`

---

### 5.3 Dashboard Page (`DashboardPage.jsx`)

**Container page with:**
- Sticky header (logo, "Logout" button)
- Tab navigation (Upload Invoices / Expenses Management)
- Outlet for tab content
- Dark mode toggle (optional)

**Layout:**
```jsx
<Header />
<Tabs>
  <Tab active={activeTab === 'upload'}>Upload Invoices</Tab>
  <Tab active={activeTab === 'expenses'}>Expenses Management</Tab>
</Tabs>
<Outlet /> {/* Renders UploadTab or ExpensesTab */}
```

---

### 5.4 Upload Tab (`UploadTab.jsx`)

**Design Reference:** `Upload Invoices.html`

**Layout:** Two-column grid (responsive)

#### Left Column: Upload Area
- **Dropzone component**
  - Drag & drop or click to upload
  - Accept: PDF, JPG, PNG (max 10MB)
  - Visual feedback on hover
- **"Process Invoice" button**
  - Triggers OCR analysis
  - Disabled until file uploaded
  - Shows loading spinner during processing
- **Upload status list**
  - Recent uploads with status (pending/success/failed)
  - Delete uploaded document

#### Right Column: Extracted Data Preview
- **Form with extracted fields:**
  - Business Name (text input)
  - Invoice Number (text input)
  - Transaction Date (date picker)
  - Business ID / VAT (text input)
  - Service Description (textarea)
  - Amount Before VAT (number input)
  - Amount After VAT (number input - highlighted)
  - VAT Amount (number, auto-calculated or editable)
- **Actions:**
  - "Discard" button → Clear form
  - "Confirm & Save" button → Create expense

**State:**
- Uploaded file
- Document ID
- OCR extraction status (pending/processing/success/error)
- Extracted data (editable)
- Form validation errors

**Flow:**
1. User uploads file → `POST /api/documents`
2. Get document ID
3. User clicks "Process Invoice" → `POST /api/documents/{id}/analyze`
4. Display extracted data in form (editable)
5. User corrects data manually
6. User clicks "Confirm & Save" → `POST /api/expenses`
7. Success → Clear form + show success message
8. Add expense to table (switch to Expenses tab or show notification)

**API Integration:**
```javascript
// Upload file
POST /api/documents
Content-Type: multipart/form-data
Response: { documentId, status: "PENDING" }

// Analyze document
POST /api/documents/{id}/analyze
Response: {
  businessName,
  transactionDate,
  amountBeforeVat,
  amountAfterVat,
  vatAmount,
  businessId,
  invoiceNumber,
  serviceDescription
}

// Create expense
POST /api/expenses
Request: {
  documentId (optional),
  businessName,
  businessId,
  invoiceNumber,
  serviceDescription,
  transactionDate,
  amountBeforeVat,
  amountAfterVat,
  vatAmount,
  category: "OTHER" (default)
}
Response: { id, ...expense }
```

---

### 5.5 Expenses Tab (`ExpensesTab.jsx`)

**Design Reference:** `Expense & Invoice Manager.html`

**Layout:**
- Filter panel (top)
- Expense table (main area)
- Pagination (bottom)

#### Filter Panel (`ExpenseFilters.jsx`)
**Filters:**
- Date Range (select: Last 30 Days, This Month, Last Quarter, Custom)
- Min Amount (number input, $ prefix)
- Max Amount (number input, $ prefix)
- Category (select: All, FOOD, VEHICLE, IT, OPERATIONS, TRAINING, OTHER)
- Business Name (text search)
- **Buttons:**
  - "Apply" (primary) → Fetch filtered data
  - "Clear" (secondary) → Reset filters

**State:**
- Filter values
- Applied filters (for API call)

#### Expense Table (`ExpenseTable.jsx`)
**Columns:**
- Business Name (with colored avatar initials)
- Date (formatted: "Oct 24, 2023")
- Amount (Ex. VAT) (monospace font)
- Amount (Inc. VAT) (bold, monospace)
- Invoice #
- Category (inline editable select dropdown)

**Row Actions:**
- Edit category (inline select)
- Delete expense (icon button - trash)

**States per row:**
- Normal
- Loading (category update in progress)
- Error (category update failed)

**Table Features:**
- Hover highlight
- Responsive (horizontal scroll on mobile)
- Empty state (no expenses found)
- Loading skeleton

#### Pagination
- Previous/Next buttons
- Current page / Total pages
- "Showing X transactions"

**State:**
- Expenses list
- Loading state
- Pagination (page, totalPages, totalCount)
- Filters
- Sort (optional: by date, amount)

**API Integration:**
```javascript
// Get expenses with filters
GET /api/expenses?category=IT&minAmount=100&maxAmount=1000&startDate=2024-01-01&endDate=2024-12-31&page=1&pageSize=20
Response: {
  data: [...expenses],
  pagination: { page, pageSize, totalCount, totalPages }
}

// Update expense category
PATCH /api/expenses/{id}
Request: { category: "IT" }
Response: { ...updated expense }

// Delete expense
DELETE /api/expenses/{id}
Response: 204 No Content
```

**Flow:**
1. Component mounts → Fetch expenses with default filters
2. User applies filters → Refetch with query params
3. User changes category → PATCH request → Update local state
4. User deletes expense → DELETE request → Remove from list
5. Pagination → Fetch next/prev page

---

## 6. Component Design

### 6.1 Common Components

#### `Button.jsx`
**Props:**
- `variant`: "primary" | "secondary" | "danger"
- `size`: "sm" | "md" | "lg"
- `loading`: boolean
- `disabled`: boolean
- `icon`: React node (Material Symbol)
- `onClick`: function
- `children`: React node

**Variants:**
- Primary: Blue background
- Secondary: White background, border
- Danger: Red background

**Loading State:**
- Show spinner
- Disable button
- Opacity 70%

---

#### `Input.jsx`
**Props:**
- `type`: "text" | "email" | "password" | "number" | "date"
- `label`: string
- `placeholder`: string
- `error`: string (validation error)
- `icon`: Material Symbol name
- `value`: any
- `onChange`: function
- `required`: boolean

**Features:**
- Label with optional required indicator
- Icon (left side)
- Error message below input
- Error state styling (red border)
- Dark mode support

---

#### `Select.jsx`
**Props:**
- `label`: string
- `options`: Array<{ value, label }>
- `value`: string
- `onChange`: function
- `error`: string
- `required`: boolean

**Features:**
- Custom styling (consistent with Input)
- Down arrow icon
- Dark mode support

---

#### `ErrorAlert.jsx`
**Props:**
- `title`: string
- `message`: string
- `onClose`: function (optional)

**Design:**
- Red background
- Error icon (Material Symbol: error)
- Close button (optional)

---

#### `LoadingSpinner.jsx`
**Props:**
- `size`: "sm" | "md" | "lg"
- `color`: "primary" | "white"

**Implementation:**
- SVG spinner with rotation animation

---

#### `Header.jsx`
**Features:**
- Logo + App name
- Logout button
- Dark mode toggle (optional)
- Sticky positioning
- Border bottom

---

### 6.2 Upload Components

#### `DropZone.jsx`
**Props:**
- `onFileSelect`: (file) => void
- `accept`: string (MIME types)
- `maxSize`: number (bytes)
- `error`: string

**Features:**
- react-dropzone integration
- Drag & drop visual states
- File validation (type, size)
- Error display

---

#### `ExtractedDataForm.jsx`
**Props:**
- `data`: OCR extracted data
- `onSave`: (formData) => void
- `onDiscard`: () => void
- `loading`: boolean

**Features:**
- Pre-populated with OCR data
- Editable fields
- Validation
- Auto-calculate VAT if needed
- Save/Discard actions

---

### 6.3 Expense Components

#### `ExpenseTable.jsx`
**Props:**
- `expenses`: Array
- `loading`: boolean
- `onCategoryChange`: (id, category) => void
- `onDelete`: (id) => void

**Features:**
- Responsive table
- Loading skeletons
- Empty state
- Row hover effects

---

#### `CategorySelect.jsx`
**Props:**
- `value`: string
- `onChange`: (value) => void
- `loading`: boolean
- `error`: boolean

**Features:**
- Inline editing
- Loading state (spinner icon)
- Error state (red border + message)
- Auto-save on change

---

## 7. State Management

### 7.1 AuthContext

**State:**
- `user`: { id, email }
- `token`: JWT string
- `isAuthenticated`: boolean
- `loading`: boolean

**Actions:**
- `login(email, password)`: Login and store token
- `register(email, password)`: Register and auto-login
- `logout()`: Clear token and redirect
- `checkAuth()`: Verify token on app load

**Storage:**
- JWT in `localStorage` (key: `auth_token`)
- Auto-attach to Axios headers

---

### 7.2 ThemeContext (Optional)

**State:**
- `theme`: "light" | "dark"

**Actions:**
- `toggleTheme()`: Switch theme
- Auto-apply class to `<html>` element

**Storage:**
- Preference in `localStorage`

---

### 7.3 React Query Setup

**Queries:**
- `useDocuments`: Fetch user documents
- `useExpenses`: Fetch expenses with filters
- `useExpense(id)`: Fetch single expense

**Mutations:**
- `useUploadDocument`: Upload file
- `useAnalyzeDocument`: Trigger OCR
- `useCreateExpense`: Create expense
- `useUpdateExpense`: Update expense (category)
- `useDeleteExpense`: Delete expense

**Cache Strategy:**
- Invalidate expenses on create/update/delete
- Cache documents for 5 minutes
- Refetch expenses on tab switch

---

## 8. API Client Setup

### Axios Instance Configuration

```javascript
// src/api/client.js
import axios from 'axios';

const apiClient = axios.create({
  baseURL: process.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Attach JWT
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: Handle 401 Unauthorized
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

---

## 9. Validation Schemas (Zod)

### Login Schema
```javascript
const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
});
```

### Register Schema
```javascript
const registerSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ['confirmPassword'],
});
```

### Expense Schema
```javascript
const expenseSchema = z.object({
  businessName: z.string().min(1, 'Business name is required'),
  transactionDate: z.string().min(1, 'Date is required'),
  amountBeforeVat: z.number().min(0, 'Amount must be positive'),
  amountAfterVat: z.number().min(0, 'Amount must be positive'),
  vatAmount: z.number().min(0, 'VAT must be positive'),
  businessId: z.string().optional(),
  invoiceNumber: z.string().optional(),
  serviceDescription: z.string().optional(),
  category: z.enum(['FOOD', 'VEHICLE', 'IT', 'OPERATIONS', 'TRAINING', 'OTHER']),
});
```

---

## 10. Styling & Theming

### TailwindCSS Configuration

```javascript
// tailwind.config.js
module.exports = {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#137fec',
        'primary-hover': '#0e62b8',
        'background-light': '#f6f7f8',
        'background-dark': '#101922',
      },
      fontFamily: {
        display: ['Inter', 'sans-serif'],
      },
      borderRadius: {
        DEFAULT: '0.25rem',
        lg: '0.5rem',
        xl: '0.75rem',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
};
```

### Design Tokens
- **Primary Color:** `#137fec` (blue)
- **Backgrounds:**
  - Light: `#f6f7f8`
  - Dark: `#101922`
- **Borders:**
  - Light: `#e7edf3`
  - Dark: `#374151`
- **Font:** Inter (Google Fonts)
- **Icons:** Material Symbols Outlined

---

## 11. Error Handling

### Error Types
1. **Network Errors** (API unreachable)
2. **Validation Errors** (client-side)
3. **API Errors** (4xx, 5xx)
4. **File Upload Errors** (size, type)

### Error Display Strategy
- **Form errors:** Inline below field (red text)
- **API errors:** Alert component at top of form
- **Global errors:** Toast notification (optional)
- **404/500:** Error page

### Error Messages
- User-friendly messages (not raw API errors)
- Retry actions for transient errors
- Validation hints (e.g., "Password must be 8+ characters")

---

## 12. Loading States

### Component-Level
- Buttons: Show spinner, disable, change text
- Forms: Disable all fields during submit
- Tables: Skeleton loaders
- Cards: Shimmer effect

### Page-Level
- Full-page spinner on initial load
- Suspense boundaries for lazy-loaded routes

---

## 13. Responsive Design

### Breakpoints
- `sm`: 640px (mobile)
- `md`: 768px (tablet)
- `lg`: 1024px (desktop)
- `xl`: 1280px (wide desktop)

### Mobile Considerations
- Stack columns on mobile (upload tab: single column)
- Horizontal scroll for table
- Touch-friendly button sizes (min 44px)
- Simplified filters (collapsible panel)

---

## 14. Performance Optimization

### Code Splitting
- Lazy load pages with React.lazy()
- Split by route

### Image Optimization
- Use WebP for decorative images
- Lazy load images

### Bundle Size
- Tree-shake unused Tailwind classes
- Use production builds
- Analyze bundle with vite-bundle-visualizer

### Caching
- React Query caching for API responses
- Service Worker for offline support (optional)

---

## 15. Accessibility (a11y)

### Requirements
- Semantic HTML (form labels, buttons)
- Keyboard navigation (tab order, focus states)
- ARIA labels for icons
- Focus trap in modals
- Screen reader support
- Color contrast (WCAG AA)

### Tools
- axe DevTools (browser extension)
- eslint-plugin-jsx-a11y

---

## 16. Testing Strategy

### Unit Tests (Vitest)
- Utility functions (formatters, validators)
- Custom hooks

### Component Tests (React Testing Library)
- Form validation
- Button states
- Error handling

### Integration Tests
- Auth flow (login → redirect)
- Upload flow (file → OCR → save)
- Filter expenses

### E2E Tests (Playwright - Optional)
- Critical user journeys
- Multi-page flows

---

## 17. Deployment

### Build for Production
```bash
npm run build
```

### Environment Variables
```
VITE_API_URL=https://api.example.com
```

### Hosting Options
- **Render** (static site)
- **Vercel** (free tier)
- **Netlify** (free tier)

### Build Output
- Static files in `dist/`
- Single-page app (SPA) routing config

---

## 18. Development Roadmap

### Phase 1: Setup (1-2 days)
- [ ] Initialize Vite + React project
- [ ] Install dependencies (Tailwind, React Router, Axios, etc.)
- [ ] Configure Tailwind
- [ ] Setup folder structure
- [ ] Create API client with interceptors

### Phase 2: Authentication (2-3 days)
- [ ] Create AuthContext
- [ ] Build LoginPage
- [ ] Build RegisterPage
- [ ] Implement ProtectedRoute
- [ ] Test auth flow

### Phase 3: Dashboard Layout (1-2 days)
- [ ] Build Header component
- [ ] Create DashboardPage with tabs
- [ ] Setup routing for tabs
- [ ] Add dark mode toggle (optional)

### Phase 4: Upload Tab (3-4 days)
- [ ] Build DropZone component
- [ ] Implement file upload API integration
- [ ] Create ExtractedDataForm
- [ ] Build document analysis flow
- [ ] Add validation and error handling
- [ ] Test end-to-end upload → OCR → save

### Phase 5: Expenses Tab (3-4 days)
- [ ] Build ExpenseFilters component
- [ ] Create ExpenseTable component
- [ ] Implement inline category editing
- [ ] Add pagination
- [ ] Integrate server-side filtering
- [ ] Add delete functionality
- [ ] Test filter combinations

### Phase 6: Polish & Optimization (2-3 days)
- [ ] Add loading states (skeletons, spinners)
- [ ] Improve error messages
- [ ] Responsive design testing
- [ ] Accessibility audit
- [ ] Performance optimization
- [ ] Cross-browser testing

### Phase 7: Deployment (1 day)
- [ ] Setup production build
- [ ] Configure environment variables
- [ ] Deploy to Render/Vercel
- [ ] Test live deployment
- [ ] Setup HTTPS and CORS

---

## 19. Dependencies

### Core
```json
{
  "react": "^18.2.0",
  "react-dom": "^18.2.0",
  "react-router-dom": "^6.20.0"
}
```

### State & Data
```json
{
  "@tanstack/react-query": "^5.15.0",
  "axios": "^1.6.0",
  "zustand": "^4.4.7" // Optional
}
```

### Forms & Validation
```json
{
  "react-hook-form": "^7.49.0",
  "zod": "^3.22.0",
  "@hookform/resolvers": "^3.3.0"
}
```

### UI & Styling
```json
{
  "tailwindcss": "^3.4.0",
  "@tailwindcss/forms": "^0.5.7",
  "@headlessui/react": "^1.7.0",
  "react-dropzone": "^14.2.0"
}
```

### Utilities
```json
{
  "date-fns": "^3.0.0",
  "clsx": "^2.0.0" // Conditional CSS classes
}
```

### Dev Dependencies
```json
{
  "@vitejs/plugin-react": "^4.2.0",
  "vite": "^5.0.0",
  "autoprefixer": "^10.4.16",
  "postcss": "^8.4.32",
  "eslint": "^8.56.0",
  "prettier": "^3.1.0"
}
```

---

## 20. Key Features Summary

### Must-Have (MVP)
- ✅ Login/Register with JWT
- ✅ Protected routes
- ✅ Upload invoice (PDF/image)
- ✅ Trigger OCR analysis
- ✅ Display extracted data (editable)
- ✅ Save expense from OCR data
- ✅ View expenses in table
- ✅ Filter expenses (category, date, amount)
- ✅ Edit category inline
- ✅ Delete expense
- ✅ Responsive design
- ✅ Dark mode

### Nice-to-Have (Post-MVP)
- 🎯 Google OAuth login
- 🎯 Export expenses to CSV
- 🎯 Expense analytics/charts
- 🎯 Bulk upload
- 🎯 Receipt image preview
- 🎯 Auto-save drafts
- 🎯 Notifications/toasts
- 🎯 Keyboard shortcuts

---

## 21. Design Consistency Checklist

Based on reference HTML files:

- [ ] Primary color: `#137fec`
- [ ] Font: Inter
- [ ] Icons: Material Symbols Outlined
- [ ] Border radius: `0.5rem` (lg), `0.75rem` (xl)
- [ ] Dark mode support
- [ ] Consistent spacing (Tailwind scale)
- [ ] Form fields: Icon + input + error message pattern
- [ ] Buttons: Loading state with spinner
- [ ] Cards: White bg, subtle shadow, rounded corners
- [ ] Error alerts: Red bg, error icon, title + message
- [ ] Input focus: Blue ring, primary border
- [ ] Table: Hover highlight, monospace for numbers

---

## 22. API Contract Summary

### Authentication
- `POST /api/auth/register` → `{ token, user }`
- `POST /api/auth/login` → `{ token, user }`
- `GET /api/auth/me` → `{ id, email }`

### Documents
- `POST /api/documents` → `{ documentId, status }`
- `POST /api/documents/{id}/analyze` → `{ ...extracted fields }`
- `GET /api/documents` → `[...documents]`
- `DELETE /api/documents/{id}` → `204`

### Expenses
- `GET /api/expenses?filters` → `{ data: [...], pagination: {...} }`
- `POST /api/expenses` → `{ id, ...expense }`
- `PATCH /api/expenses/{id}` → `{ ...updated expense }`
- `DELETE /api/expenses/{id}` → `204`

---

## 23. Notes

- **UI Priority:** Functionality over aesthetics (per spec)
- **OCR Accuracy:** Manual correction is expected workflow
- **Filters:** All filtering happens server-side (no client filtering)
- **Security:** Never expose API keys or secrets
- **File Storage:** Files stored on backend, frontend only handles upload
- **Categories:** Fixed enum (FOOD, VEHICLE, IT, OPERATIONS, TRAINING, OTHER)

---

## 24. Questions to Clarify (Optional)

Before implementation, consider clarifying:
1. Password reset flow? (Email service needed)
2. Google OAuth integration priority?
3. Multi-file upload or single file only?
4. Default category for new expenses?
5. Pagination page size (20, 50, 100)?
6. Date format preference (US vs. ISO)?
7. Currency symbol ($, €, etc.)?
8. Can users delete uploaded documents?
9. Can users edit amounts after expense creation?
10. Mobile app needed later? (affects architecture)

---

**End of Frontend React Implementation Plan**
