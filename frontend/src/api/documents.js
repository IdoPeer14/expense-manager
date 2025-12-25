import apiClient from './client';

/**
 * Upload a document file
 * @param {File} file - Document file to upload
 * @returns {Promise<{documentId: string, status: string}>}
 */
export const uploadDocument = async (file) => {
  const formData = new FormData();
  formData.append('file', file);

  // Don't set Content-Type manually - let Axios set it with the boundary
  const response = await apiClient.post('/documents', formData);

  return response.data;
};

/**
 * Analyze a document with OCR
 * @param {string} documentId - Document ID
 * @returns {Promise<Object>} Extracted data from OCR
 */
export const analyzeDocument = async (documentId) => {
  // OCR processing can take longer, increase timeout to 2 minutes
  const response = await apiClient.post(`/documents/${documentId}/analyze`, null, {
    timeout: 120000, // 2 minutes
  });
  return response.data;
};

/**
 * Get all user documents
 * @returns {Promise<Array>} List of documents
 */
export const getDocuments = async () => {
  const response = await apiClient.get('/documents');
  return response.data;
};

/**
 * Delete a document
 * @param {string} documentId - Document ID
 * @returns {Promise<void>}
 */
export const deleteDocument = async (documentId) => {
  const response = await apiClient.delete(`/documents/${documentId}`);
  return response.data;
};
