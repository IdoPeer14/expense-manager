using System.Text.RegularExpressions;
using ExpenseManager.Api.Services.Validators;

namespace ExpenseManager.Api.Services.Extractors;

public class BusinessIdExtractor : BaseFieldExtractor<string>
{
    // Priority 1: Israeli Company Number (ח.פ.)
    private static readonly Regex Priority1Pattern = new(
        @"(?:ח\.פ\.|ח״פ|חפ)\s*[:\-]?\s*([\d\-]{8,10})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 2: Licensed Dealer (עוסק מורשה)
    private static readonly Regex Priority2Pattern = new(
        @"(?:ע\.מ\.|עוסק\s*מורשה|ע״מ)\s*[:\-]?\s*([\d\-]{8,10})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 3: Licensed Partnership (עוסק פטור)
    private static readonly Regex Priority3Pattern = new(
        @"(?:ע\.פ\.|עוסק\s*פטור)\s*[:\-]?\s*([\d\-]{8,10})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 4: VAT Number (explicit)
    private static readonly Regex Priority4Pattern = new(
        @"(?:VAT\s*(?:No|Number|ID)|מע[""״]מ\s*(?:מס|מספר))\s*[:\-]?\s*([\d\-]{8,12})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 5: Generic Tax ID / Company ID
    private static readonly Regex Priority5Pattern = new(
        @"(?:Company\s*ID|Tax\s*ID|Business\s*ID|Company\s*No)\s*[:\-]?\s*([\d\-]{8,12})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 6: Standalone 9-digit ID (Israeli standard)
    private static readonly Regex Priority6Pattern = new(
        @"\b(\d{9})\b",
        RegexOptions.Compiled
    );

    public override string FieldName => "BusinessId";

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
            (Pattern: Priority1Pattern, Name: "Priority1_CompanyNumber", Confidence: 1.0f),
            (Pattern: Priority2Pattern, Name: "Priority2_LicensedDealer", Confidence: 1.0f),
            (Pattern: Priority3Pattern, Name: "Priority3_Partnership", Confidence: 1.0f),
            (Pattern: Priority4Pattern, Name: "Priority4_VATNumber", Confidence: 0.95f),
            (Pattern: Priority5Pattern, Name: "Priority5_GenericID", Confidence: 0.9f),
            (Pattern: Priority6Pattern, Name: "Priority6_StandaloneID", Confidence: 0.6f)
        };

        foreach (var attempt in attempts)
        {
            var match = attempt.Pattern.Match(normalizedText);

            if (!match.Success)
                continue;

            string rawValue = match.Groups[1].Value;
            string normalized = BusinessIdValidator.NormalizeBusinessID(rawValue);

            // Validate length
            if (normalized.Length < 8 || normalized.Length > 12)
                continue;

            // For Israeli IDs (8-9 digits), validate checksum
            float confidenceMultiplier = 1.0f;
            if (normalized.Length == 9 || normalized.Length == 8)
            {
                if (!BusinessIdValidator.ValidateIsraeliBusinessID(normalized))
                {
                    // Lower confidence if checksum fails, but don't reject
                    confidenceMultiplier = 0.7f;
                }
            }

            result.Value = normalized;
            result.PatternUsed = attempt.Name;
            result.Factors.PatternPriority = attempt.Confidence * confidenceMultiplier;
            result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
            result.Confidence = result.Factors.CalculateOverallConfidence();

            // If confidence is acceptable, return
            if (result.Confidence >= 0.6f)
                return result;
        }

        return result;
    }

    protected override float GetPositionConfidenceForField(float position)
    {
        // Business ID typically appears in top 40% of document
        return position < 0.4f ? 1.0f : 0.85f;
    }
}
