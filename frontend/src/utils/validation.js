import { z } from 'zod';

// Login Schema
export const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address'),
  password: z
    .string()
    .min(1, 'Password is required'),
});

// Register Schema
export const registerSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Za-z]/, 'Password must contain at least one letter')
    .regex(/[0-9]/, 'Password must contain at least one number'),
  confirmPassword: z
    .string()
    .min(1, 'Please confirm your password'),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ['confirmPassword'],
});

// Expense Schema
export const expenseSchema = z.object({
  businessName: z
    .string()
    .min(1, 'Business name is required')
    .max(200, 'Business name is too long'),
  transactionDate: z
    .string()
    .min(1, 'Transaction date is required'),
  amountBeforeVat: z
    .number({ invalid_type_error: 'Amount must be a number' })
    .min(0, 'Amount must be positive'),
  amountAfterVat: z
    .number({ invalid_type_error: 'Amount must be a number' })
    .min(0, 'Amount must be positive'),
  vatAmount: z
    .number({ invalid_type_error: 'VAT must be a number' })
    .min(0, 'VAT must be positive'),
  businessId: z.string().optional(),
  invoiceNumber: z.string().optional(),
  serviceDescription: z.string().optional(),
  category: z.enum(['FOOD', 'VEHICLE', 'IT', 'OPERATIONS', 'TRAINING', 'OTHER']),
});

// Expense Filter Schema
export const expenseFilterSchema = z.object({
  category: z.string().optional(),
  minAmount: z.number().optional(),
  maxAmount: z.number().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  businessName: z.string().optional(),
});
