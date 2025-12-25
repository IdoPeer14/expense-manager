import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Document, Page, pdfjs } from 'react-pdf';

// Configure PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

const FilePreview = ({ file, className = '' }) => {
  const { t } = useTranslation();
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [error, setError] = useState(null);
  const [scale, setScale] = useState(1.0);
  const [pageWidth, setPageWidth] = useState(600);
  const containerRef = useRef(null);

  useEffect(() => {
    const updateWidth = () => {
      if (containerRef.current) {
        const containerWidth = containerRef.current.offsetWidth;
        setPageWidth(Math.min(containerWidth - 32, 800));
      }
    };

    updateWidth();
    window.addEventListener('resize', updateWidth);
    return () => window.removeEventListener('resize', updateWidth);
  }, []);

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'ArrowLeft') {
        goToPrevPage();
      } else if (e.key === 'ArrowRight') {
        goToNextPage();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [pageNumber, numPages]);

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
    setPageNumber((prev) => Math.min(prev + 1, numPages || prev));
  };

  const goToFirstPage = () => {
    setPageNumber(1);
  };

  const goToLastPage = () => {
    if (numPages) setPageNumber(numPages);
  };

  const zoomIn = () => {
    setScale((prev) => Math.min(prev + 0.25, 3));
  };

  const zoomOut = () => {
    setScale((prev) => Math.max(prev - 0.25, 0.5));
  };

  const resetZoom = () => {
    setScale(1.0);
  };

  const handlePageInputChange = (e) => {
    const value = parseInt(e.target.value);
    if (value >= 1 && value <= numPages) {
      setPageNumber(value);
    }
  };

  if (!file) {
    return null;
  }

  // Create object URL for the file
  const fileURL = URL.createObjectURL(file);
  const fileType = file.type;
  const fileSizeKB = (file.size / 1024).toFixed(1);

  // Render image preview
  if (fileType.startsWith('image/')) {
    return (
      <div className={`flex flex-col gap-4 ${className}`} ref={containerRef}>
        {/* File Info Header */}
        <div className="flex items-center justify-between px-4 py-3 bg-slate-50 dark:bg-slate-800 rounded-lg border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <span className="material-symbols-outlined text-primary text-[24px]">image</span>
            <div>
              <p className="text-sm font-semibold text-slate-900 dark:text-white truncate max-w-[200px]">
                {file.name}
              </p>
              <p className="text-xs text-slate-500 dark:text-slate-400">{fileSizeKB} KB</p>
            </div>
          </div>
          <div className="flex gap-2">
            <button
              onClick={zoomIn}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
              title="Zoom in"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">zoom_in</span>
            </button>
            <button
              onClick={zoomOut}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
              title="Zoom out"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">zoom_out</span>
            </button>
            <button
              onClick={resetZoom}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
              title="Reset zoom"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">fit_screen</span>
            </button>
          </div>
        </div>

        {/* Image Container */}
        <div className="relative w-full border border-slate-200 dark:border-slate-700 rounded-lg overflow-auto bg-slate-50 dark:bg-slate-800 shadow-sm">
          <div className="flex justify-center items-center min-h-[400px] p-4">
            <img
              src={fileURL}
              alt="File preview"
              style={{ transform: `scale(${scale})`, transition: 'transform 0.2s ease' }}
              className="max-w-full h-auto rounded"
            />
          </div>
        </div>
      </div>
    );
  }

  // Render PDF preview
  if (fileType === 'application/pdf') {
    return (
      <div className={`flex flex-col gap-4 ${className}`} ref={containerRef}>
        {/* File Info Header */}
        <div className="flex items-center justify-between px-4 py-3 bg-slate-50 dark:bg-slate-800 rounded-lg border border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            <span className="material-symbols-outlined text-red-500 text-[24px]">picture_as_pdf</span>
            <div>
              <p className="text-sm font-semibold text-slate-900 dark:text-white truncate max-w-[200px]">
                {file.name}
              </p>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                {fileSizeKB} KB {numPages && `• ${numPages} ${numPages === 1 ? 'page' : 'pages'}`}
              </p>
            </div>
          </div>

          {/* Zoom Controls */}
          <div className="flex items-center gap-2">
            <button
              onClick={zoomOut}
              disabled={scale <= 0.5}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title="Zoom out"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">zoom_out</span>
            </button>
            <span className="text-xs font-medium text-slate-600 dark:text-slate-400 min-w-[45px] text-center">
              {Math.round(scale * 100)}%
            </span>
            <button
              onClick={zoomIn}
              disabled={scale >= 3}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title="Zoom in"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">zoom_in</span>
            </button>
            <button
              onClick={resetZoom}
              className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
              title="Reset zoom"
            >
              <span className="material-symbols-outlined text-[20px] text-slate-600 dark:text-slate-400">fit_screen</span>
            </button>
          </div>
        </div>

        {error ? (
          <div className="w-full p-12 text-center border border-red-200 dark:border-red-800 rounded-lg bg-red-50 dark:bg-red-900/20">
            <span className="material-symbols-outlined text-5xl text-red-500 dark:text-red-400 mb-4 block">
              error
            </span>
            <p className="text-red-800 dark:text-red-200 font-semibold mb-2">Failed to load PDF</p>
            <p className="text-sm text-red-600 dark:text-red-300">{error}</p>
          </div>
        ) : (
          <>
            {/* PDF Viewer */}
            <div className="relative w-full border border-slate-200 dark:border-slate-700 rounded-lg overflow-auto bg-slate-100 dark:bg-slate-800 shadow-sm">
              <div className="flex justify-center items-center min-h-[500px] p-4">
                <Document
                  file={fileURL}
                  onLoadSuccess={onDocumentLoadSuccess}
                  onLoadError={onDocumentLoadError}
                  loading={
                    <div className="flex flex-col items-center justify-center p-16 gap-4">
                      <div className="relative">
                        <div className="animate-spin h-12 w-12 border-4 border-primary border-t-transparent rounded-full"></div>
                        <span className="material-symbols-outlined absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 text-primary text-[20px]">
                          description
                        </span>
                      </div>
                      <p className="text-sm text-slate-600 dark:text-slate-400 font-medium">
                        Loading PDF...
                      </p>
                    </div>
                  }
                >
                  <div style={{ transform: `scale(${scale})`, transformOrigin: 'top center', transition: 'transform 0.2s ease' }}>
                    <Page
                      pageNumber={pageNumber}
                      renderTextLayer={false}
                      renderAnnotationLayer={false}
                      width={pageWidth}
                      className="shadow-lg"
                    />
                  </div>
                </Document>
              </div>
            </div>

            {/* PDF Navigation Controls */}
            {numPages && numPages > 0 && (
              <div className="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 bg-slate-50 dark:bg-slate-800 rounded-lg border border-slate-200 dark:border-slate-700">
                {/* Navigation Buttons */}
                <div className="flex items-center gap-2">
                  <button
                    onClick={goToFirstPage}
                    disabled={pageNumber <= 1}
                    className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    title="First page"
                  >
                    <span className="material-symbols-outlined text-[20px] text-slate-700 dark:text-slate-300">first_page</span>
                  </button>
                  <button
                    onClick={goToPrevPage}
                    disabled={pageNumber <= 1}
                    className="flex items-center gap-1 px-3 py-2 rounded-lg bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm font-medium shadow-sm"
                  >
                    <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                    <span className="hidden sm:inline">Previous</span>
                  </button>

                  <button
                    onClick={goToNextPage}
                    disabled={pageNumber >= numPages}
                    className="flex items-center gap-1 px-3 py-2 rounded-lg bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm font-medium shadow-sm"
                  >
                    <span className="hidden sm:inline">Next</span>
                    <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                  </button>
                  <button
                    onClick={goToLastPage}
                    disabled={pageNumber >= numPages}
                    className="p-2 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    title="Last page"
                  >
                    <span className="material-symbols-outlined text-[20px] text-slate-700 dark:text-slate-300">last_page</span>
                  </button>
                </div>

                {/* Page Indicator */}
                <div className="flex items-center gap-3">
                  <span className="text-sm text-slate-600 dark:text-slate-400">Page</span>
                  <input
                    type="number"
                    min="1"
                    max={numPages}
                    value={pageNumber}
                    onChange={handlePageInputChange}
                    className="w-16 px-2 py-1 text-center text-sm font-semibold rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 text-slate-900 dark:text-white focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none"
                  />
                  <span className="text-sm text-slate-600 dark:text-slate-400">of {numPages}</span>
                </div>

                {/* Keyboard hint */}
                <div className="hidden lg:flex items-center gap-2 text-xs text-slate-500 dark:text-slate-500">
                  <span className="px-2 py-1 rounded bg-slate-200 dark:bg-slate-700 font-mono">←</span>
                  <span className="px-2 py-1 rounded bg-slate-200 dark:bg-slate-700 font-mono">→</span>
                  <span>to navigate</span>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    );
  }

  // Fallback for unsupported file types
  return (
    <div className={`w-full p-12 text-center border-2 border-dashed border-slate-300 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 ${className}`}>
      <span className="material-symbols-outlined text-6xl text-slate-400 mb-4 block">
        description
      </span>
      <p className="text-slate-700 dark:text-slate-300 font-semibold mb-1">Preview not available</p>
      <p className="text-sm text-slate-500 dark:text-slate-400">This file type is not supported for preview</p>
    </div>
  );
};

export default FilePreview;
