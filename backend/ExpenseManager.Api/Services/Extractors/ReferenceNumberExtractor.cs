using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public enum ReferenceType
{
    Unknown,
    OrderId,
    BookingId,
    ConfirmationNumber,
    TransactionId
}

public class ReferenceNumber
{
    public string? Value { get; set; }
    public ReferenceType Type { get; set; }
}

public class ReferenceNumberExtractor : BaseFieldExtractor<ReferenceNumber>
{
    // Priority 1: Order ID
    private static readonly Regex OrderIdPattern = new(
        @"(?:Order\s*(?:ID|No|Number)|הזמנה\s*(?:מס|מספר))\s*[:\-#]?\s*([A-Z0-9\-]{4,20})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 2: Booking ID / Confirmation Number
    private static readonly Regex BookingIdPattern = new(
        @"(?:Booking\s*(?:ID|No|Number)|Confirmation|אישור\s*(?:מס|מספר))\s*[:\-#]?\s*([A-Z0-9\-]{4,20})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 3: Reference Number
    private static readonly Regex ReferencePattern = new(
        @"(?:Reference|Ref|אסמכתא)\s*[:\-#]?\s*([A-Z0-9\-]{4,20})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 4: Transaction ID
    private static readonly Regex TransactionIdPattern = new(
        @"(?:Transaction\s*(?:ID|No)|עסקה\s*מס)\s*[:\-#]?\s*([A-Z0-9\-]{4,20})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public override string FieldName => "ReferenceNumber";

    public override ExtractionResult<ReferenceNumber> Extract(string normalizedText)
    {
        var result = new ExtractionResult<ReferenceNumber>
        {
            Confidence = 0.0f
        };

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        // Try patterns in priority order
        var attempts = new[]
        {
            (Pattern: OrderIdPattern, Name: "OrderId", Type: ReferenceType.OrderId, Confidence: 0.95f),
            (Pattern: BookingIdPattern, Name: "BookingId", Type: ReferenceType.BookingId, Confidence: 0.95f),
            (Pattern: ReferencePattern, Name: "Reference", Type: ReferenceType.ConfirmationNumber, Confidence: 0.9f),
            (Pattern: TransactionIdPattern, Name: "TransactionId", Type: ReferenceType.TransactionId, Confidence: 0.9f)
        };

        foreach (var attempt in attempts)
        {
            var match = attempt.Pattern.Match(normalizedText);

            if (!match.Success)
                continue;

            string refNumber = match.Groups[1].Value.Trim();

            // Validation: check length
            if (refNumber.Length < 4 || refNumber.Length > 20)
                continue;

            result.Value = new ReferenceNumber
            {
                Value = refNumber,
                Type = attempt.Type
            };

            result.PatternUsed = attempt.Name;
            result.Factors.PatternPriority = attempt.Confidence;
            result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
            result.Confidence = result.Factors.CalculateOverallConfidence();

            return result;
        }

        return result;
    }

    protected override float GetPositionConfidenceForField(float position)
    {
        // Reference numbers can appear anywhere, slight preference for top
        return position < 0.5f ? 1.0f : 0.9f;
    }
}
