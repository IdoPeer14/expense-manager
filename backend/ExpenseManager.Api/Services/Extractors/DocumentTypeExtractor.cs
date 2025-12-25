using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public enum DocumentType
{
    Unknown,
    TaxInvoice,
    Invoice,
    Receipt
}

public class DocumentTypeExtractor : BaseFieldExtractor<DocumentType>
{
    private static readonly Regex DocumentTypePattern = new(
        @"(?:חשבונית\s*מס|חשבונית|קבלה|חש[׳']?\s*מס|tax\s*invoice|invoice|receipt)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public override string FieldName => "DocumentType";

    public override ExtractionResult<DocumentType> Extract(string normalizedText)
    {
        var result = new ExtractionResult<DocumentType>
        {
            Value = DocumentType.Unknown,
            Confidence = 0.0f
        };

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        var match = DocumentTypePattern.Match(normalizedText);

        if (!match.Success)
            return result;

        string matchedText = match.Value.ToLowerInvariant();
        DocumentType docType = DocumentType.Unknown;

        // Priority order: Tax Invoice > Invoice > Receipt
        if (matchedText.Contains("חשבונית מס") ||
            matchedText.Contains("חש") && matchedText.Contains("מס") ||
            matchedText.Contains("tax") && matchedText.Contains("invoice"))
        {
            docType = DocumentType.TaxInvoice;
        }
        else if (matchedText.Contains("חשבונית") || matchedText.Contains("invoice"))
        {
            docType = DocumentType.Invoice;
        }
        else if (matchedText.Contains("קבלה") || matchedText.Contains("receipt"))
        {
            docType = DocumentType.Receipt;
        }

        if (docType != DocumentType.Unknown)
        {
            result.Value = docType;
            result.PatternUsed = "DocumentTypePattern";
            result.Factors.PatternPriority = 1.0f;
            result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
            result.Confidence = result.Factors.CalculateOverallConfidence();
        }

        return result;
    }

    protected override float GetPositionConfidenceForField(float position)
    {
        // Document type should appear in top 30% of document
        return position < 0.3f ? 1.0f : 0.85f;
    }
}
