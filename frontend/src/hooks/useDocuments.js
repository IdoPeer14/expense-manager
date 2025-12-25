import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as documentsApi from '../api/documents';

// Query keys
const QUERY_KEYS = {
  documents: ['documents'],
};

/**
 * Hook to fetch all documents
 */
export const useDocuments = () => {
  return useQuery({
    queryKey: QUERY_KEYS.documents,
    queryFn: documentsApi.getDocuments,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

/**
 * Hook to upload a document
 */
export const useUploadDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: documentsApi.uploadDocument,
    onSuccess: () => {
      // Invalidate documents query to refetch
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.documents });
    },
  });
};

/**
 * Hook to analyze a document with OCR
 */
export const useAnalyzeDocument = () => {
  return useMutation({
    mutationFn: documentsApi.analyzeDocument,
  });
};

/**
 * Hook to delete a document
 */
export const useDeleteDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: documentsApi.deleteDocument,
    onSuccess: () => {
      // Invalidate documents query to refetch
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.documents });
    },
  });
};
