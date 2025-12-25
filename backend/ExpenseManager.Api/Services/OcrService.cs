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

        // Mark as processing
        document.ParseStatus = DocumentParseStatus.PROCESSING;
        await _db.SaveChangesAsync();

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

                // Log the raw OCR text for debugging
                _logger.LogInformation("Raw OCR Text:\n{OcrText}", ocrText);

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
                // Create cache directory for converted images
                var cacheDir = Path.Combine(Path.GetTempPath(), "expense-manager-pdf-cache");
                Directory.CreateDirectory(cacheDir);

                // Generate cache key based on PDF file hash
                var pdfHash = GetFileHash(pdfPath);
                var cachedImagePath = Path.Combine(cacheDir, $"{pdfHash}.png");

                // Check if cached image exists and is newer than PDF
                if (File.Exists(cachedImagePath))
                {
                    var pdfLastModified = File.GetLastWriteTimeUtc(pdfPath);
                    var cacheLastModified = File.GetLastWriteTimeUtc(cachedImagePath);

                    if (cacheLastModified >= pdfLastModified)
                    {
                        _logger.LogInformation("Using cached PDF image: {CachedPath}", cachedImagePath);

                        // Copy to temp location to maintain existing cleanup behavior
                        var tempPath = Path.Combine(Path.GetTempPath(), $"pdf_convert_{Guid.NewGuid()}.png");
                        File.Copy(cachedImagePath, tempPath);
                        return tempPath;
                    }
                }

                // Convert PDF to PNG with optimized settings
                var outputBaseName = Path.Combine(Path.GetTempPath(), $"pdf_convert_{Guid.NewGuid()}");
                var outputImagePath = $"{outputBaseName}.png";

                // Use pdftoppm with optimized parameters:
                // - Lower DPI (150 instead of default 300) for faster conversion
                // - JPEG format is faster but PNG is better for OCR
                // - -f 1 -singlefile: first page only
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pdftoppm",
                    Arguments = $"\"{pdfPath}\" \"{outputBaseName}\" -png -f 1 -singlefile -r 150 -gray",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _logger.LogInformation("Running optimized pdftoppm: {Command} {Arguments}", startInfo.FileName, startInfo.Arguments);

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

                // Cache the converted image for future use
                try
                {
                    File.Copy(outputImagePath, cachedImagePath, overwrite: true);
                    _logger.LogDebug("Cached converted image: {CachedPath}", cachedImagePath);
                }
                catch (Exception cacheEx)
                {
                    _logger.LogWarning(cacheEx, "Failed to cache converted image, continuing anyway");
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

    private string GetFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
