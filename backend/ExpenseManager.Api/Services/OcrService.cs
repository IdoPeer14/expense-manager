using System.Diagnostics;
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
            // Convert PDF to image if needed
            string fileToProcess = document.StoragePath;
            string? tempImagePath = null;

            if (IsPdfFile(document.StoragePath))
            {
                _logger.LogInformation("Converting PDF to image for OCR: {FilePath}", document.StoragePath);
                tempImagePath = await ConvertPdfToImageAsync(document.StoragePath);
                fileToProcess = tempImagePath;
                _logger.LogInformation("PDF converted to image: {ImagePath}", tempImagePath);
            }

            try
            {
                // Run Tesseract OCR
                var ocrText = await RunTesseractOcr(fileToProcess);

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
            finally
            {
                // Clean up temporary image file
                if (tempImagePath != null && File.Exists(tempImagePath))
                {
                    try
                    {
                        File.Delete(tempImagePath);
                        _logger.LogDebug("Deleted temporary image: {TempPath}", tempImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary image: {TempPath}", tempImagePath);
                    }
                }
            }
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
                // First check TESSDATA_PREFIX environment variable
                var envTessdata = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");

                var possiblePaths = new List<string>();

                // Add env variable path first if set
                if (!string.IsNullOrEmpty(envTessdata))
                {
                    possiblePaths.Add(envTessdata);
                }

                // Add standard locations
                possiblePaths.AddRange(new[]
                {
                    "/opt/homebrew/share/tessdata",             // macOS Homebrew (Apple Silicon)
                    "/usr/local/share/tessdata",                // macOS Homebrew (Intel)
                    "/usr/share/tesseract-ocr/tessdata",        // Standard Debian/Ubuntu location (apt install)
                    "/usr/share/tessdata",                      // Alternative Linux location
                    "/usr/share/tesseract-ocr/5.00/tessdata",   // Versioned Tesseract 5.x
                    "/usr/share/tesseract-ocr/4.00/tessdata",   // Versioned Tesseract 4.x
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata")  // Custom location
                });

                string? tessdataPath = null;
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        tessdataPath = path;
                        _logger.LogInformation("Found tessdata at: {TessdataPath}", path);
                        break;
                    }
                    else
                    {
                        _logger.LogDebug("Tessdata not found at: {Path}", path);
                    }
                }

                if (tessdataPath == null)
                {
                    throw new DirectoryNotFoundException(
                        $"Tessdata directory not found. TESSDATA_PREFIX={envTessdata}. Searched: {string.Join(", ", possiblePaths)}");
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

    private bool IsPdfFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".pdf";
    }

    private async Task<string> ConvertPdfToImageAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Create temporary directory for conversion
                var tempDir = Path.Combine(Path.GetTempPath(), "expense-manager-ocr");
                Directory.CreateDirectory(tempDir);

                // Generate unique filename for output
                var outputBaseName = Path.Combine(tempDir, $"pdf_convert_{Guid.NewGuid()}");
                var outputImagePath = $"{outputBaseName}.png";

                // Use pdftoppm to convert PDF to PNG (first page only)
                // pdftoppm is available via poppler-utils package
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pdftoppm",
                    Arguments = $"\"{pdfPath}\" \"{outputBaseName}\" -png -f 1 -singlefile",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _logger.LogInformation("Running pdftoppm: {Command} {Arguments}", startInfo.FileName, startInfo.Arguments);

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start pdftoppm process");
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"pdftoppm failed with exit code {process.ExitCode}. Error: {error}");
                }

                if (!File.Exists(outputImagePath))
                {
                    throw new FileNotFoundException(
                        $"Converted image not found at {outputImagePath}. pdftoppm output: {output}");
                }

                _logger.LogInformation("Successfully converted PDF to image: {ImagePath}", outputImagePath);
                return outputImagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF to image conversion failed for: {PdfPath}", pdfPath);
                throw new InvalidOperationException($"Failed to convert PDF to image: {ex.Message}", ex);
            }
        });
    }
}
