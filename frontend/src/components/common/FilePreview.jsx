import { useState } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';

// Configure PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

const FilePreview = ({ file, className = '' }) => {
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [error, setError] = useState(null);

  const onDocumentLoadSuccess = ({ numPages }) => {
    setNumPages(numPages);
    setError(null);
  };

  const onDocumentLoadError = (error) => {
    console.error('Error loading PDF:', error);
    setError('Failed to load PDF file');
  };

  const goToPrevPage = () => {
    setPageNumber((prev) => Math.max(prev - 1, 1));
  };

  const goToNextPage = () => {
    setPageNumber((prev) => Math.min(prev + 1, numPages));
  };

  if (!file) {
    return null;
  }

  // Create object URL for the file
  const fileURL = URL.createObjectURL(file);
  const fileType = file.type;

  // Render image preview
  if (fileType.startsWith('image/')) {
    return (
      <div className={`flex flex-col items-center gap-4 ${className}`}>
        <div className="w-full border border-slate-200 dark:border-slate-700 rounded-lg overflow-hidden bg-slate-50 dark:bg-slate-800">
          <img
            src={fileURL}
            alt="File preview"
            className="w-full h-auto max-h-[600px] object-contain"
          />
        </div>
      </div>
    );
  }

  // Render PDF preview
  if (fileType === 'application/pdf') {
    return (
      <div className={`flex flex-col items-center gap-4 ${className}`}>
        {error ? (
          <div className="w-full p-8 text-center border border-red-200 dark:border-red-800 rounded-lg bg-red-50 dark:bg-red-900/20">
            <span className="material-symbols-outlined text-4xl text-red-500 dark:text-red-400 mb-2">
              error
            </span>
            <p className="text-red-800 dark:text-red-200 font-medium">{error}</p>
          </div>
        ) : (
          <>
            <div className="w-full border border-slate-200 dark:border-slate-700 rounded-lg overflow-hidden bg-slate-50 dark:bg-slate-800 flex justify-center">
              <Document
                file={fileURL}
                onLoadSuccess={onDocumentLoadSuccess}
                onLoadError={onDocumentLoadError}
                loading={
                  <div className="flex items-center justify-center p-12">
                    <div className="animate-spin h-8 w-8 border-3 border-primary border-t-transparent rounded-full"></div>
                  </div>
                }
              >
                <Page
                  pageNumber={pageNumber}
                  renderTextLayer={false}
                  renderAnnotationLayer={false}
                  className="max-w-full"
                  width={Math.min(window.innerWidth * 0.9, 600)}
                />
              </Document>
            </div>

            {/* PDF Navigation Controls */}
            {numPages && numPages > 1 && (
              <div className="flex items-center justify-center gap-4 w-full">
                <button
                  onClick={goToPrevPage}
                  disabled={pageNumber <= 1}
                  className="flex items-center gap-1 px-4 py-2 rounded-lg bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm font-medium"
                >
                  <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                  Previous
                </button>

                <span className="text-sm font-medium text-slate-600 dark:text-slate-400 px-4 py-2 rounded-lg bg-slate-100 dark:bg-slate-800">
                  Page {pageNumber} of {numPages}
                </span>

                <button
                  onClick={goToNextPage}
                  disabled={pageNumber >= numPages}
                  className="flex items-center gap-1 px-4 py-2 rounded-lg bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm font-medium"
                >
                  Next
                  <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                </button>
              </div>
            )}
          </>
        )}
      </div>
    );
  }

  // Fallback for unsupported file types
  return (
    <div className={`w-full p-8 text-center border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 ${className}`}>
      <span className="material-symbols-outlined text-4xl text-slate-400 mb-2">
        description
      </span>
      <p className="text-slate-600 dark:text-slate-400 font-medium">Preview not available for this file type</p>
    </div>
  );
};

export default FilePreview;
