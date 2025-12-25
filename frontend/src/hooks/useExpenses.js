import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as expensesApi from '../api/expenses';

// Query keys
const QUERY_KEYS = {
  expenses: (filters) => ['expenses', filters],
  expense: (id) => ['expense', id],
};

/**
 * Hook to fetch expenses with filters
 * @param {Object} filters - Filter parameters
 */
export const useExpenses = (filters = {}) => {
  return useQuery({
    queryKey: QUERY_KEYS.expenses(filters),
    queryFn: () => expensesApi.getExpenses(filters),
    keepPreviousData: true, // Keep old data while fetching new data
  });
};

/**
 * Hook to fetch a single expense
 * @param {string} expenseId - Expense ID
 */
export const useExpense = (expenseId) => {
  return useQuery({
    queryKey: QUERY_KEYS.expense(expenseId),
    queryFn: () => expensesApi.getExpense(expenseId),
    enabled: !!expenseId, // Only fetch if ID is provided
  });
};

/**
 * Hook to create an expense
 */
export const useCreateExpense = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: expensesApi.createExpense,
    onSuccess: () => {
      // Invalidate all expense queries to refetch
      queryClient.invalidateQueries({ queryKey: ['expenses'] });
    },
  });
};

/**
 * Hook to update an expense
 */
export const useUpdateExpense = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ expenseId, data }) => expensesApi.updateExpense(expenseId, data),
    onSuccess: (_, variables) => {
      // Invalidate specific expense and all expenses list
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.expense(variables.expenseId) });
      queryClient.invalidateQueries({ queryKey: ['expenses'] });
    },
  });
};

/**
 * Hook to delete an expense
 */
export const useDeleteExpense = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: expensesApi.deleteExpense,
    onSuccess: () => {
      // Invalidate all expense queries to refetch
      queryClient.invalidateQueries({ queryKey: ['expenses'] });
    },
  });
};
