using System.Diagnostics;
using System.Text.Json;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Models;
using Microsoft.EntityFrameworkCore;

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
                _logger.LogInformation("Running Tesseract OCR on: {FilePath}", filePath);

                // Use tesseract command-line tool
                // Output to stdout with Hebrew and English languages
                var startInfo = new ProcessStartInfo
                {
                    FileName = "tesseract",
                    Arguments = $"\"{filePath}\" stdout -l heb+eng",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _logger.LogDebug("Tesseract command: {Command} {Arguments}", startInfo.FileName, startInfo.Arguments);

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start tesseract process");
                }

                // Read output
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Tesseract failed with exit code {ExitCode}. Error: {Error}",
                        process.ExitCode, error);
                    throw new InvalidOperationException(
                        $"Tesseract failed with exit code {process.ExitCode}. Error: {error}");
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogWarning("Tesseract returned empty output for file: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogInformation("Tesseract successfully extracted {Length} characters", output.Length);
                }

                return output;
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
