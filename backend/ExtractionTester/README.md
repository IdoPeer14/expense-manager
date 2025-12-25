# Extraction Tester - Invoice/Receipt Field Extraction

A comprehensive testing tool for the regex-based invoice extraction engine.

## Features

- ✅ Batch processes all receipts in a directory
- ✅ Runs OCR (Tesseract) with Hebrew + English support
- ✅ Extracts structured fields using the new extraction pipeline
- ✅ Displays results with confidence scores
- ✅ Exports results to JSON for analysis
- ✅ Beautiful console output with progress indicators

## Prerequisites

1. **Tesseract OCR** installed with Hebrew language support:
   ```bash
   # macOS
   brew install tesseract tesseract-lang

   # Ubuntu/Debian
   sudo apt install tesseract-ocr tesseract-ocr-heb tesseract-ocr-eng
   ```

2. **.NET 8.0+** SDK installed

## Usage

### Quick Start

1. Place receipt files (PDF, PNG, JPG, etc.) in the `../Receipts` directory

2. Run the tester:
   ```bash
   cd backend/ExtractionTester
   dotnet run
   ```

### Custom Directory

```bash
dotnet run /path/to/your/receipts
```

### Output

The tester will:
1. Process each receipt file
2. Display extraction results with confidence indicators:
   - 🟢 High confidence (≥95%)
   - 🟡 Good confidence (≥80%)
   - 🟠 Medium confidence (≥60%)
   - 🔴 Low confidence (>0%)
   - ⚫ Not extracted

3. Save detailed results to `extraction_results.json`

## Example Output

```
╔══════════════════════════════════════════════════════════════════╗
║     REGEX-BASED INVOICE EXTRACTION ENGINE - TEST SUITE          ║
╚══════════════════════════════════════════════════════════════════╝

📂 Receipts directory: /Users/you/Receipts
📄 Found 3 file(s) to process

🔍 Initializing Tesseract OCR...
✅ Using tessdata: /opt/homebrew/share/tessdata

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📄 [1/3] invoice_123.pdf
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔍 Running OCR (Hebrew + English)...
   ✅ OCR completed (confidence: 89.3%)
   📝 Extracted 847 characters

⚙️  Running extraction pipeline...
   ✅ Extraction completed (overall confidence: 92.5%)

┌─────────────────────────────────────────────────────────────────┐
│                     EXTRACTION RESULTS                          │
└─────────────────────────────────────────────────────────────────┘

  🟢 Document Type       TaxInvoice                     (100%)
  🟢 Invoice Number      123456                         (100%)
  🟢 Transaction Date    2024-12-25                     (100%)
  🟡 Business Name       ABC Tech Ltd.                  (85%)
  🟢 Business ID         513123456                      (95%)

  🟢 Amount (Before VAT) ₪850.00                        (95%)
  🟢 VAT Amount          ₪144.50                        (95%)
  🟢 Amount (After VAT)  ₪994.50                        (100%)

📈 Overall Confidence: [████████████████████] 92.5%

╔══════════════════════════════════════════════════════════════════╗
║                          SUMMARY                                 ║
╚══════════════════════════════════════════════════════════════════╝

✅ Successful: 3/3
❌ Failed: 0/3

📊 Average extraction confidence: 91.2%

💾 Saving results to extraction_results.json...
   ✅ Saved to /path/to/extraction_results.json

✨ Done!
```

## Supported File Formats

- PDF (`.pdf`)
- PNG (`.png`)
- JPEG (`.jpg`, `.jpeg`)
- TIFF (`.tif`, `.tiff`)

## JSON Output

The `extraction_results.json` file contains:
```json
[
  {
    "FileName": "invoice_123.pdf",
    "OcrConfidence": 0.893,
    "OcrTextLength": 847,
    "OcrText": "...",
    "ExtractionResult": {
      "BusinessName": "ABC Tech Ltd.",
      "TransactionDate": "2024-12-25T00:00:00",
      "AmountBeforeVat": 850.00,
      "AmountAfterVat": 994.50,
      "VatAmount": 144.50,
      "BusinessId": "513123456",
      "InvoiceNumber": "123456",
      "DocumentType": "TaxInvoice",
      "BusinessNameConfidence": 0.85,
      "TransactionDateConfidence": 1.0,
      ...
    }
  }
]
```

## Troubleshooting

### Tessdata Not Found
```
❌ Tessdata not found. Please install Tesseract
```
**Solution:** Install Tesseract with language packs (see Prerequisites)

### No Receipt Files Found
```
❌ No receipt files found!
```
**Solution:** Add PDF or image files to the `../Receipts` directory

### Low Confidence Scores
- Check OCR quality (OCR confidence should be >70%)
- Ensure receipts are clear and well-lit
- Verify receipts contain expected fields
- Hebrew text should be properly rendered

## Next Steps

1. **Analyze Results:** Review `extraction_results.json` for patterns
2. **Tune Patterns:** Adjust regex patterns in extractors for better accuracy
3. **Add Test Cases:** Create a test suite with known good/bad examples
4. **Compare Performance:** Benchmark against old extraction method

## Architecture

```
Receipt File (PDF/Image)
    ↓
Tesseract OCR (heb+eng)
    ↓
Text Normalization
    ↓
Field Extraction Pipeline
    ├─ Document Type
    ├─ Invoice Number
    ├─ Date
    ├─ Business Name
    ├─ Business ID
    ├─ Amounts (Total, VAT, Before VAT)
    └─ Reference Numbers
    ↓
Confidence Scoring
    ↓
JSON Export + Console Display
```

## Contributing

To improve extraction accuracy:
1. Run the tester on your receipt collection
2. Identify low-confidence extractions
3. Update regex patterns in `/Services/Extractors/`
4. Re-test and validate improvements
