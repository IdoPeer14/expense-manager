using System.Text.RegularExpressions;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services.Extractors;
using ExpenseManager.Api.Services.Normalizers;

namespace ExpenseManager.Api.Services;

public class InvoiceParser
{
    private readonly TextNormalizer _textNormalizer;
    private readonly DocumentTypeExtractor _documentTypeExtractor;
    private readonly InvoiceNumberExtractor _invoiceNumberExtractor;
    private readonly DateExtractor _dateExtractor;
    private readonly BusinessNameExtractor _businessNameExtractor;
    private readonly BusinessIdExtractor _businessIdExtractor;
    private readonly AmountExtractor _amountExtractor;
    private readonly ReferenceNumberExtractor _referenceNumberExtractor;

    public InvoiceParser()
    {
        _textNormalizer = new TextNormalizer();
        _documentTypeExtractor = new DocumentTypeExtractor();
        _invoiceNumberExtractor = new InvoiceNumberExtractor();
        _dateExtractor = new DateExtractor();
        _businessNameExtractor = new BusinessNameExtractor();
        _businessIdExtractor = new BusinessIdExtractor();
        _amountExtractor = new AmountExtractor();
        _referenceNumberExtractor = new ReferenceNumberExtractor();
    }

    public ParsedInvoiceData ParseInvoiceFields(string ocrText)
    {
        var result = new ParsedInvoiceData();

        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return result;
        }

        // Step 1: Normalize text
        string normalizedText = _textNormalizer.Normalize(ocrText);

        // Step 2: Extract independent fields
        var docTypeResult = _documentTypeExtractor.Extract(normalizedText);
        var invoiceNumberResult = _invoiceNumberExtractor.Extract(normalizedText);
        var dateResult = _dateExtractor.Extract(normalizedText);
        var businessNameResult = _businessNameExtractor.Extract(normalizedText);
        var businessIdResult = _businessIdExtractor.Extract(normalizedText);
        var referenceNumberResult = _referenceNumberExtractor.Extract(normalizedText);

        // Step 3: Extract monetary fields
        var monetaryAmounts = _amountExtractor.ExtractAmounts(normalizedText);

        // Step 4: Populate result with extracted values and confidence scores
        if (docTypeResult.IsSuccess && docTypeResult.Value != null)
        {
            result.DocumentType = (DocumentType)docTypeResult.Value;
            result.DocumentTypeConfidence = docTypeResult.Confidence;
        }

        if (invoiceNumberResult.IsSuccess)
        {
            result.InvoiceNumber = invoiceNumberResult.Value;
            result.InvoiceNumberConfidence = invoiceNumberResult.Confidence;
        }

        if (dateResult.IsSuccess)
        {
            result.TransactionDate = dateResult.Value;
            result.TransactionDateConfidence = dateResult.Confidence;
        }

        if (businessNameResult.IsSuccess)
        {
            result.BusinessName = businessNameResult.Value;
            result.BusinessNameConfidence = businessNameResult.Confidence;
        }

        if (businessIdResult.IsSuccess)
        {
            result.BusinessId = businessIdResult.Value;
            result.BusinessIdConfidence = businessIdResult.Confidence;
        }

        if (referenceNumberResult.IsSuccess && referenceNumberResult.Value != null)
        {
            result.ReferenceNumber = referenceNumberResult.Value.Value;
            result.ReferenceType = referenceNumberResult.Value.Type;
            result.ReferenceNumberConfidence = referenceNumberResult.Confidence;
        }

        // Monetary amounts
        if (monetaryAmounts.Total.IsSuccess)
        {
            result.AmountAfterVat = monetaryAmounts.Total.Value;
            result.AmountAfterVatConfidence = monetaryAmounts.Total.Confidence;
        }

        if (monetaryAmounts.VAT.IsSuccess)
        {
            result.VatAmount = monetaryAmounts.VAT.Value;
            result.VatAmountConfidence = monetaryAmounts.VAT.Confidence;
        }

        if (monetaryAmounts.BeforeVAT.IsSuccess)
        {
            result.AmountBeforeVat = monetaryAmounts.BeforeVAT.Value;
            result.AmountBeforeVatConfidence = monetaryAmounts.BeforeVAT.Confidence;
        }

        // Fallback to legacy method for service description
        result.ServiceDescription = ExtractServiceDescription(normalizedText);

        return result;
    }

    // Legacy service description extractor (to be refactored into dedicated extractor)
    private string? ExtractServiceDescription(string text)
    {
        // First try explicit description patterns
        var explicitPatterns = new[]
        {
            @"(?:Description|תיאור)\s*[:\-]?\s*\n\s*(.+)",
            @"(?:Service|שירות)\s*[:\-]?\s*(.+)"
        };

        foreach (var pattern in explicitPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var desc = match.Groups[1].Value.Trim();
                if (desc.Length > 3 && desc.Length < 200)
                {
                    return desc;
                }
            }
        }

        // Fallback: Look for lines that might describe services
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 10 && trimmed.Length < 200 &&
                (trimmed.Contains("שירות") || trimmed.Contains("service", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("מוצר") || trimmed.Contains("product", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("development", StringComparison.OrdinalIgnoreCase)))
            {
                return trimmed;
            }
        }

        return null;
    }
}
