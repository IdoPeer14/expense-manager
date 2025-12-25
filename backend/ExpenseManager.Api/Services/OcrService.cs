using System.Text.Json;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Models;
using Microsoft.EntityFrameworkCore;
using Tesseract;

namespace ExpenseManager.Api.Services;

public class OcrService
{
    private readonly AppDbContext _db;
    private readonly InvoiceParser _invoiceParser;
    private readonly ILogger<OcrService> _logger;

    public OcrService(AppDbContext db, InvoiceParser invoiceParser, ILogger<OcrService> logger)
    {
        _db = db;
        _invoiceParser = invoiceParser;
        _logger = logger;
    }

    public async Task<ParsedInvoiceData?> ProcessDocumentAsync(Guid documentId)
    {
        var document = await _db.Documents.FindAsync(documentId);

        if (document == null)
        {
            throw new FileNotFoundException($"Document {documentId} not found in database.");
        }

        if (!File.Exists(document.StoragePath))
        {
            throw new FileNotFoundException($"File not found: {document.StoragePath}");
        }

        try
        {
            // Run Tesseract OCR
            var ocrText = await RunTesseractOcr(document.StoragePath);

            // Parse invoice fields
            var parsedData = _invoiceParser.ParseInvoiceFields(ocrText);

            // Update document with OCR results
            document.OcrText = ocrText;
            document.ParsedJson = JsonSerializer.Serialize(parsedData);
            document.ParseStatus = DocumentParseStatus.SUCCESS;
            document.ParseError = null;

            await _db.SaveChangesAsync();

            return parsedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed for document {DocumentId}", documentId);

            // Update document with error
            document.ParseStatus = DocumentParseStatus.FAILED;
            document.ParseError = ex.Message;

            await _db.SaveChangesAsync();

            throw;
        }
    }

    private async Task<string> RunTesseractOcr(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Initialize Tesseract with Hebrew and English
                // tessdata location in Docker: /usr/share/tesseract-ocr/4.00/tessdata
                var tessdataPath = "/usr/share/tesseract-ocr/4.00/tessdata";

                // Fallback for local development (macOS/Windows)
                if (!Directory.Exists(tessdataPath))
                {
                    tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                }

                using var engine = new TesseractEngine(tessdataPath, "heb+eng", EngineMode.Default);

                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);

                var text = page.GetText();

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tesseract OCR failed for file: {FilePath}", filePath);
                throw new InvalidOperationException($"OCR processing failed: {ex.Message}", ex);
            }
        });
    }
}
