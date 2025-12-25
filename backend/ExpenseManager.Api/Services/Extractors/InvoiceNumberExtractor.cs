using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public class InvoiceNumberExtractor : BaseFieldExtractor<string>
{
    // Priority 1: Explicit invoice number with label (numeric only)
    private static readonly Regex Priority1Pattern = new(
        @"(?:חשבונית|invoice)\s*(?:מס[׳']|מספר|#|num|no\.?|number)?\s*[:\-]?\s*(\d{4,12})(?!\w)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 2: Receipt number with label (numeric only)
    private static readonly Regex Priority2Pattern = new(
        @"(?:קבלה|receipt)\s*(?:מס[׳']|מספר|#|num|no\.?|number)?\s*[:\-]?\s*(\d{4,12})(?!\w)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 3: Generic number label (numeric only)
    private static readonly Regex Priority3Pattern = new(
        @"(?:מס[׳']|מספר|#|no\.?)\s*[:\-]?\s*(\d{4,12})(?!\w)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 4: Alphanumeric invoice number (e.g., "JZMYWEKA-0003", "INV-2024-001")
    private static readonly Regex Priority4Pattern = new(
        @"(?:חשבונית|invoice|קבלה|receipt)\s*(?:מס[׳']|מספר|#|num|no\.?|number)\s*[:\-]?\s*([A-Za-z0-9]+-[A-Za-z0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 5: Very simple alphanumeric pattern (e.g., "ABC-123", "JZMYWEKA-0003")
    private static readonly Regex Priority5Pattern = new(
        @"(?:invoice|receipt|חשבונית|קבלה).{0,20}?([A-Z]{2,}[A-Z0-9]*-[0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 6: Standalone alphanumeric ID (flexible format)
    private static readonly Regex Priority6Pattern = new(
        @"\b([A-Z]{2,}[A-Z0-9]*-[A-Z0-9]{2,})\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 7: Standalone numeric ID (near document keywords)
    private static readonly Regex Priority7Pattern = new(
        @"(?<=(?:חשבונית|invoice|קבלה|receipt).{0,50})(\d{8,12})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly string[] ExcludeKeywords = new[]
    {
        "הזמנה", "booking", "order", "המחאה", "check", "תאריך", "date"
    };

    public override string FieldName => "InvoiceNumber";

    public override ExtractionResult<string> Extract(string normalizedText)
    {
        var result = new ExtractionResult<string>
        {
            Confidence = 0.0f
        };

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        // Try patterns in priority order
        var attempts = new[]
        {
            (Pattern: Priority1Pattern, Name: "Priority1_ExplicitInvoice", Confidence: 1.0f, IsAlphanumeric: false),
            (Pattern: Priority2Pattern, Name: "Priority2_Receipt", Confidence: 1.0f, IsAlphanumeric: false),
            (Pattern: Priority4Pattern, Name: "Priority4_AlphanumericExplicit", Confidence: 0.98f, IsAlphanumeric: true),
            (Pattern: Priority5Pattern, Name: "Priority5_AlphanumericNear", Confidence: 0.92f, IsAlphanumeric: true),
            (Pattern: Priority3Pattern, Name: "Priority3_GenericNumber", Confidence: 0.85f, IsAlphanumeric: false),
            (Pattern: Priority6Pattern, Name: "Priority6_AlphanumericStandalone", Confidence: 0.75f, IsAlphanumeric: true),
            (Pattern: Priority7Pattern, Name: "Priority7_StandaloneID", Confidence: 0.7f, IsAlphanumeric: false)
        };

        foreach (var attempt in attempts)
        {
            var match = attempt.Pattern.Match(normalizedText);

            if (!match.Success)
                continue;

            string invoiceNumber = match.Groups[1].Value;

            // Alphanumeric validation
            if (attempt.IsAlphanumeric)
            {
                // For alphanumeric, length can be longer (up to 20 characters)
                if (invoiceNumber.Length < 5 || invoiceNumber.Length > 20)
                    continue;

                // Exclude if near exclusion keywords
                if (IsNearKeyword(normalizedText, match.Index, ExcludeKeywords, 50))
                    continue;
            }
            else
            {
                // Numeric validation: check length
                if (invoiceNumber.Length < 4 || invoiceNumber.Length > 12)
                    continue;

                // Exclude if near exclusion keywords
                if (IsNearKeyword(normalizedText, match.Index, ExcludeKeywords, 50))
                    continue;

                // Exclude if it looks like a date (8 digits in DDMMYYYY format)
                if (invoiceNumber.Length == 8 && LooksLikeDate(invoiceNumber))
                    continue;

                // Exclude if it looks like a business ID (9 digits)
                if (invoiceNumber.Length == 9 && !IsNearKeyword(normalizedText, match.Index,
                    new[] { "חשבונית", "invoice", "קבלה", "receipt", "מס", "no", "number" }, 30))
                    continue;
            }

            result.Value = invoiceNumber;
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
        // Invoice number should appear in top 40% of document
        return position < 0.4f ? 1.0f : 0.8f;
    }

    private bool LooksLikeDate(string number)
    {
        if (number.Length != 8)
            return false;

        // Check if it could be DDMMYYYY
        if (int.TryParse(number.Substring(0, 2), out int day) &&
            int.TryParse(number.Substring(2, 2), out int month) &&
            int.TryParse(number.Substring(4, 4), out int year))
        {
            return day >= 1 && day <= 31 && month >= 1 && month <= 12 && year >= 2000 && year <= 2100;
        }

        return false;
    }
}
