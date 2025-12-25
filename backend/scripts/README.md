# Backend Scripts

Utility scripts for testing and development.

## Testing Scripts

### OCR Testing
- **`test-ocr-local.sh`** - Test OCR functionality locally with sample files
- **`test-ocr-direct.sh`** - Direct OCR testing without API
- **`convert-pdfs-to-images.sh`** - Convert PDF files to images for OCR processing

### Extraction Testing
- **`run-extraction-test.sh`** - Run the ExtractionTester project on sample receipts
- **`test-all-receipts.sh`** - Test extraction on all receipt files in a directory

### API Testing
- **`test-api.sh`** - Test API endpoints (auth, documents, expenses)
- **`test-document-upload.sh`** - Test document upload and OCR processing endpoints

## Usage

All scripts should be run from the backend directory:

```bash
cd backend
./scripts/test-api.sh
```

## Requirements

- Backend API running (for API test scripts)
- PostgreSQL database configured
- Sample receipt files (in `../docs/samples/`)
- curl (for API tests)
- .NET 8.0 SDK (for extraction tests)

## Notes

Some scripts may require updating paths or API URLs depending on your environment configuration.
