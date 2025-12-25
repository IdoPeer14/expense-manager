// API Configuration
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Expense Categories
export const EXPENSE_CATEGORIES = [
  { value: 'FOOD', label: 'Food & Drink' },
  { value: 'VEHICLE', label: 'Vehicle' },
  { value: 'IT', label: 'IT Services' },
  { value: 'OPERATIONS', label: 'Operations' },
  { value: 'TRAINING', label: 'Training/Education' },
  { value: 'OTHER', label: 'Other' },
];

// File Upload Constraints
export const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
export const ACCEPTED_FILE_TYPES = {
  'application/pdf': ['.pdf'],
  'image/jpeg': ['.jpg', '.jpeg'],
  'image/png': ['.png'],
};

// Authentication
export const AUTH_TOKEN_KEY = 'auth_token';

// Pagination
export const DEFAULT_PAGE_SIZE = 20;
