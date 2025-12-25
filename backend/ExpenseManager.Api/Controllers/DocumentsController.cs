using ExpenseManager.Api.Data;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : BaseAuthController
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _fileStorage;
    private readonly OcrService _ocrService;

    public DocumentsController(AppDbContext db, FileStorageService fileStorage, OcrService ocrService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _ocrService = ocrService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (!_fileStorage.ValidateMimeType(file.ContentType))
        {
            return BadRequest(new { error = "Invalid file type. Only PDF, JPG, and PNG are allowed." });
        }

        if (!_fileStorage.ValidateFileSize(file.Length))
        {
            return BadRequest(new { error = "File size exceeds maximum allowed (10MB)." });
        }

        var userId = GetCurrentUserId();
        var storagePath = await _fileStorage.SaveFileAsync(file, userId);

        var document = new Document
        {
            UserId = userId,
            OriginalFileName = file.FileName,
            MimeType = file.ContentType,
            StoragePath = storagePath
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            documentId = document.Id,
            status = document.ParseStatus.ToString()
        });
    }

    [HttpPost("{id}/analyze")]
    public async Task<IActionResult> AnalyzeDocument(Guid id)
    {
        var userId = GetCurrentUserId();
        var document = await _db.Documents.FindAsync(id);

        if (document == null || document.UserId != userId)
        {
            return NotFound(new { error = "Document not found." });
        }

        try
        {
            var parsedData = await _ocrService.ProcessDocumentAsync(id);

            return Ok(new
            {
                vendorName = parsedData?.BusinessName,
                date = parsedData?.TransactionDate?.ToString("yyyy-MM-dd"),
                totalAmount = parsedData?.AmountAfterVat ?? parsedData?.AmountBeforeVat ?? 0,
                currency = "USD",
                description = parsedData?.ServiceDescription,
                // Include additional fields for reference
                invoiceNumber = parsedData?.InvoiceNumber,
                businessId = parsedData?.BusinessId,
                amountBeforeVat = parsedData?.AmountBeforeVat,
                amountAfterVat = parsedData?.AmountAfterVat,
                vatAmount = parsedData?.VatAmount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "OCR processing failed",
                message = ex.Message
            });
        }
    }
}
