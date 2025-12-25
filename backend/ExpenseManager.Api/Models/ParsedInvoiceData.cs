using ExpenseManager.Api.Services.Extractors;

namespace ExpenseManager.Api.Models;

public class ParsedInvoiceData
{
    public string? BusinessName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? AmountBeforeVat { get; set; }
    public decimal? AmountAfterVat { get; set; }
    public decimal? VatAmount { get; set; }
    public string? BusinessId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ServiceDescription { get; set; }

    // New fields
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;
    public string? ReferenceNumber { get; set; }
    public ReferenceType ReferenceType { get; set; } = ReferenceType.Unknown;

    // Confidence scores
    public float BusinessNameConfidence { get; set; }
    public float TransactionDateConfidence { get; set; }
    public float AmountBeforeVatConfidence { get; set; }
    public float AmountAfterVatConfidence { get; set; }
    public float VatAmountConfidence { get; set; }
    public float BusinessIdConfidence { get; set; }
    public float InvoiceNumberConfidence { get; set; }
    public float DocumentTypeConfidence { get; set; }
    public float ReferenceNumberConfidence { get; set; }

    // Overall extraction quality
    public float OverallConfidence
    {
        get
        {
            var confidences = new[]
            {
                BusinessNameConfidence,
                TransactionDateConfidence,
                AmountAfterVatConfidence,
                InvoiceNumberConfidence
            };

            // Average of non-zero confidences
            var nonZero = confidences.Where(c => c > 0).ToArray();
            return nonZero.Length > 0 ? nonZero.Average() : 0.0f;
        }
    }
}
