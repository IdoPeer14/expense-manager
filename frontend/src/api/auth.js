import apiClient from './client';

/**
 * Register a new user
 * @param {Object} data - Registration data
 * @param {string} data.email - User email
 * @param {string} data.password - User password
 * @returns {Promise<{token: string, user: Object}>}
 */
export const register = async (data) => {
  const response = await apiClient.post('/auth/register', data);
  return response.data;
};

/**
 * Login user
 * @param {Object} data - Login credentials
 * @param {string} data.email - User email
 * @param {string} data.password - User password
 * @returns {Promise<{token: string, user: Object}>}
 */
export const login = async (data) => {
  const response = await apiClient.post('/auth/login', data);
  return response.data;
};

/**
 * Get current authenticated user
 * @returns {Promise<Object>} User object
 */
export const getCurrentUser = async () => {
  const response = await apiClient.get('/auth/me');
  return response.data;
};

/**
 * Logout user (client-side only for now)
 */
export const logout = () => {
  // Clear token from localStorage
  // Backend logout endpoint can be added later if needed
  return Promise.resolve();
};
