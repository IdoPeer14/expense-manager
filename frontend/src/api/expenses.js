import apiClient from './client';

/**
 * Get expenses with optional filters
 * @param {Object} params - Query parameters
 * @param {string} params.category - Category filter
 * @param {number} params.minAmount - Minimum amount
 * @param {number} params.maxAmount - Maximum amount
 * @param {string} params.startDate - Start date (ISO format)
 * @param {string} params.endDate - End date (ISO format)
 * @param {string} params.businessName - Business name search
 * @param {number} params.page - Page number
 * @param {number} params.pageSize - Items per page
 * @returns {Promise<{data: Array, pagination: Object}>}
 */
export const getExpenses = async (params = {}) => {
  // Remove undefined/null values
  const cleanParams = Object.fromEntries(
    Object.entries(params).filter(([_, value]) => value != null && value !== '')
  );

  const response = await apiClient.get('/expenses', {
    params: cleanParams,
  });

  return response.data;
};

/**
 * Get a single expense by ID
 * @param {string} expenseId - Expense ID
 * @returns {Promise<Object>} Expense object
 */
export const getExpense = async (expenseId) => {
  const response = await apiClient.get(`/expenses/${expenseId}`);
  return response.data;
};

/**
 * Create a new expense
 * @param {Object} data - Expense data
 * @returns {Promise<Object>} Created expense
 */
export const createExpense = async (data) => {
  const response = await apiClient.post('/expenses', data);
  return response.data;
};

/**
 * Update an expense
 * @param {string} expenseId - Expense ID
 * @param {Object} data - Updated expense data
 * @returns {Promise<Object>} Updated expense
 */
export const updateExpense = async (expenseId, data) => {
  const response = await apiClient.patch(`/expenses/${expenseId}`, data);
  return response.data;
};

/**
 * Delete an expense
 * @param {string} expenseId - Expense ID
 * @returns {Promise<void>}
 */
export const deleteExpense = async (expenseId) => {
  const response = await apiClient.delete(`/expenses/${expenseId}`);
  return response.data;
};
