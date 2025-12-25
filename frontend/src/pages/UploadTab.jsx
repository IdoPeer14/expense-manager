import { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useUploadDocument, useAnalyzeDocument } from '../hooks/useDocuments';
import { useCreateExpense } from '../hooks/useExpenses';
import { MAX_FILE_SIZE, ACCEPTED_FILE_TYPES } from '../utils/constants';
import ErrorAlert from '../components/common/ErrorAlert';
import ExtractedDataForm from '../components/upload/ExtractedDataForm';
import FilePreview from '../components/common/FilePreview';

const UploadTab = () => {
  const { t } = useTranslation();
  const [selectedFile, setSelectedFile] = useState(null);
  const [extractedData, setExtractedData] = useState(null);
  const [error, setError] = useState('');
  const [isDragging, setIsDragging] = useState(false);
  const [success, setSuccess] = useState('');
  const fileInputRef = useRef(null);

  const uploadMutation = useUploadDocument();
  const analyzeMutation = useAnalyzeDocument();
  const createExpenseMutation = useCreateExpense();

  const validateFile = (file) => {
    if (!file) return t('upload.noFileSelected');

    // Check file type
    const allowedTypes = Object.keys(ACCEPTED_FILE_TYPES);
    if (!allowedTypes.includes(file.type)) {
      return t('upload.invalidFileType');
    }

    // Check file size
    if (file.size > MAX_FILE_SIZE) {
      return t('upload.fileTooLarge', { size: MAX_FILE_SIZE / 1024 / 1024 });
    }

    return null;
  };

  const handleFileSelect = (file) => {
    setError('');
    const validationError = validateFile(file);

    if (validationError) {
      setError(validationError);
      return;
    }

    setSelectedFile(file);
    setExtractedData(null);
  };

  const handleFileInputChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  const handleDragEnter = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDragOver = (e) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    const file = e.dataTransfer.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  const handleProcessInvoice = async () => {
    if (!selectedFile) return;

    setError('');
    setSuccess('');
    setExtractedData(null);

    try {
      // Step 1: Upload the file
      const uploadResult = await uploadMutation.mutateAsync(selectedFile);

      // Step 2: Analyze the document
      const analyzeResult = await analyzeMutation.mutateAsync(uploadResult.documentId);

      setExtractedData(analyzeResult);
    } catch (err) {
      setError(err.message || t('upload.processFailed'));
    }
  };

  const handleSaveExpense = async (formData) => {
    setError('');
    setSuccess('');

    try {
      // Transform frontend data to match backend API expectations
      const expenseData = {
        businessName: formData.vendorName,
        transactionDate: formData.date,
        amountBeforeVat: formData.totalAmount * 0.83, // Estimate (assuming ~17% VAT)
        amountAfterVat: formData.totalAmount,
        vatAmount: formData.totalAmount * 0.17, // Estimate
        serviceDescription: formData.description,
        category: formData.category,
        businessId: null,
        invoiceNumber: null,
        documentId: null,
      };

      await createExpenseMutation.mutateAsync(expenseData);
      setSuccess(t('upload.successMessage'));

      // Reset form after short delay
      setTimeout(() => {
        setSelectedFile(null);
        setExtractedData(null);
        setSuccess('');
      }, 2000);
    } catch (err) {
      setError(err.message || t('upload.saveFailed'));
    }
  };

  const handleDiscard = () => {
    setExtractedData(null);
    setSelectedFile(null);
    setError('');
    setSuccess('');
  };

  const isProcessing = uploadMutation.isPending || analyzeMutation.isPending;
  const isSaving = createExpenseMutation.isPending;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 sm:gap-6 items-start">
      {/* Left Column: Upload Area */}
      <div className="lg:col-span-5 flex flex-col gap-3 sm:gap-4">
        <div className="bg-white dark:bg-slate-900 rounded-xl p-4 sm:p-6 shadow-sm border border-slate-200 dark:border-slate-800">
          <h3 className="text-base sm:text-lg font-bold mb-3 sm:mb-4 text-slate-900 dark:text-white">{t('upload.title')}</h3>

          {/* Error Alert */}
          {error && (
            <div className="mb-4">
              <ErrorAlert
                title={t('upload.uploadError')}
                message={error}
                onClose={() => setError('')}
              />
            </div>
          )}

          {/* Success Alert */}
          {success && (
            <div className="mb-4 rounded-lg bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 p-4">
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-green-600 dark:text-green-400">
                  check_circle
                </span>
                <p className="text-sm font-medium text-green-800 dark:text-green-200">
                  {success}
                </p>
              </div>
            </div>
          )}

          {/* Dropzone */}
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.jpg,.jpeg,.png"
            onChange={handleFileInputChange}
            className="hidden"
          />

          <div
            onClick={() => fileInputRef.current?.click()}
            onDragEnter={handleDragEnter}
            onDragLeave={handleDragLeave}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            className={`flex flex-col items-center justify-center gap-3 sm:gap-4 rounded-lg border-2 border-dashed ${
              isDragging
                ? 'border-primary bg-primary/5 dark:bg-primary/10'
                : 'border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50'
            } hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors px-4 sm:px-6 py-8 sm:py-12 cursor-pointer`}
          >
            <div className="h-10 w-10 sm:h-12 sm:w-12 rounded-full bg-primary/10 flex items-center justify-center text-primary mb-1 sm:mb-2">
              <span className="material-symbols-outlined text-2xl sm:text-3xl">
                {selectedFile ? 'check_circle' : 'upload_file'}
              </span>
            </div>
            <div className="flex flex-col items-center gap-1 text-center px-2">
              {selectedFile ? (
                <>
                  <p className="text-slate-900 dark:text-white text-sm sm:text-base font-bold break-all">
                    {selectedFile.name}
                  </p>
                  <p className="text-slate-500 dark:text-slate-400 text-xs">
                    {(selectedFile.size / 1024).toFixed(1)} KB
                  </p>
                </>
              ) : (
                <>
                  <p className="text-slate-900 dark:text-white text-sm sm:text-base font-bold">
                    {t('upload.dragDrop')}
                  </p>
                  <p className="text-slate-500 dark:text-slate-400 text-xs">
                    {t('upload.fileTypes')}
                  </p>
                </>
              )}
            </div>
          </div>

          {/* Process Button */}
          <div className="mt-4 sm:mt-6">
            <button
              onClick={handleProcessInvoice}
              disabled={!selectedFile || isProcessing}
              className="flex w-full cursor-pointer items-center justify-center gap-2 rounded-lg h-11 sm:h-12 bg-primary hover:bg-primary-hover text-white text-sm sm:text-base font-bold shadow-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isProcessing ? (
                <>
                  <div className="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full"></div>
                  <span>{t('upload.processing')}</span>
                </>
              ) : (
                <>
                  <span className="material-symbols-outlined text-[20px]">rocket_launch</span>
                  <span>{t('upload.processButton')}</span>
                </>
              )}
            </button>
          </div>
        </div>

        {/* File Preview */}
        {selectedFile && (
          <div className="bg-white dark:bg-slate-900 rounded-xl p-4 sm:p-6 shadow-sm border border-slate-200 dark:border-slate-800">
            <h3 className="text-base sm:text-lg font-bold mb-3 sm:mb-4 text-slate-900 dark:text-white">{t('upload.preview')}</h3>
            <FilePreview file={selectedFile} />
          </div>
        )}
      </div>

      {/* Right Column: Extracted Data Preview */}
      <div className="lg:col-span-7 flex flex-col gap-3 sm:gap-4">
        <div className="bg-white dark:bg-slate-900 rounded-xl p-4 sm:p-6 lg:p-8 shadow-sm border border-slate-200 dark:border-slate-800">
          <div className="flex items-center justify-between mb-4 sm:mb-6 pb-3 sm:pb-4 border-b border-slate-200 dark:border-slate-800">
            <h3 className="text-base sm:text-lg font-bold flex items-center gap-2 text-slate-900 dark:text-white">
              <span className="material-symbols-outlined text-primary text-[20px] sm:text-[24px]">data_info_alert</span>
              <span className="hidden sm:inline">{t('upload.extractedData')}</span>
              <span className="sm:hidden text-sm">{t('upload.extractedData')}</span>
            </h3>
            <span className="px-2 sm:px-3 py-1 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 text-[10px] sm:text-xs font-bold uppercase tracking-wider">
              {t('upload.preview')}
            </span>
          </div>

          {extractedData ? (
            <ExtractedDataForm
              data={extractedData}
              onSave={handleSaveExpense}
              onDiscard={handleDiscard}
              loading={isSaving}
            />
          ) : (
            <div className="text-center py-8 sm:py-12 text-slate-400 dark:text-slate-500">
              <span className="material-symbols-outlined text-5xl sm:text-6xl mb-3 sm:mb-4 opacity-50">description</span>
              <p className="text-xs sm:text-sm px-4">{t('upload.uploadInstruction')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default UploadTab;
