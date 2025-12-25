using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public class BusinessNameExtractor : BaseFieldExtractor<string>
{
    // Priority 1: Explicit business name label
    private static readonly Regex Priority1Pattern = new(
        @"(?:שם\s*העסק|business\s*name|company\s*name)\s*[:\-]?\s*\n?\s*([^\n]{3,80})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 2: Limited company designations (improved to capture full name)
    private static readonly Regex Priority2Pattern = new(
        @"([א-ת\s\w&,\.]+)\s*(?:בע[״""]מ|Ltd\.?|Inc\.?|LLC|PBC|L\.L\.C\.)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 3: Company designation at end of line (captures more context)
    private static readonly Regex Priority3Pattern = new(
        @"^([A-Za-z][A-Za-z\s&,\.\-]+(?:Ltd\.?|Inc\.?|LLC|PBC|L\.L\.C\.))\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline
    );

    private static readonly string[] ExcludeKeywords = new[]
    {
        "חשבונית", "invoice", "receipt", "קבלה", "total", "סה\"כ", "סה״כ",
        "vat", "מע\"מ", "מע״מ", "date", "תאריך", "amount", "סכום",
        "http://", "https://", "@", "payment", "subtotal", "description",
        "quantity", "price", "item", "service"
    };

    public override string FieldName => "BusinessName";

    public override ExtractionResult<string> Extract(string normalizedText)
    {
        var result = new ExtractionResult<string>
        {
            Confidence = 0.0f
        };

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        // Try Priority 1: Explicit label
        var match = Priority1Pattern.Match(normalizedText);
        if (match.Success)
        {
            string name = match.Groups[1].Value.Trim();
            if (IsValidBusinessName(name))
            {
                result.Value = name;
                result.PatternUsed = "Priority1_ExplicitLabel";
                result.Factors.PatternPriority = 1.0f;
                result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
                result.Confidence = result.Factors.CalculateOverallConfidence();
                return result;
            }
        }

        // Try Priority 2: Company designation patterns (better matching)
        match = Priority3Pattern.Match(normalizedText);
        if (match.Success)
        {
            string name = match.Groups[1].Value.Trim();
            if (IsValidBusinessName(name))
            {
                result.Value = name;
                result.PatternUsed = "Priority2_CompanyDesignationLine";
                result.Factors.PatternPriority = 0.96f;
                result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
                result.Confidence = result.Factors.CalculateOverallConfidence();
                return result;
            }
        }

        // Try Priority 3: Company designation (inline)
        match = Priority2Pattern.Match(normalizedText);
        if (match.Success)
        {
            string name = match.Value.Trim();
            if (IsValidBusinessName(name))
            {
                result.Value = name;
                result.PatternUsed = "Priority3_CompanyDesignation";
                result.Factors.PatternPriority = 0.95f;
                result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
                result.Confidence = result.Factors.CalculateOverallConfidence();
                return result;
            }
        }

        // Priority 4: Heuristic - first substantial line (improved logic)
        var lines = normalizedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int lineIndex = 0;
        foreach (var line in lines.Take(20))
        {
            string trimmed = line.Trim();
            lineIndex++;

            // Skip very short lines (likely not company name)
            if (trimmed.Length < 5)
                continue;

            // Skip lines with excluded keywords
            if (ContainsExcludeKeywords(trimmed))
                continue;

            // Skip lines that are just numbers or dates
            if (Regex.IsMatch(trimmed, @"^\d+[\d\/\.\-\s]*$"))
                continue;

            // Skip URLs and emails
            if (trimmed.Contains("http") || trimmed.Contains("www.") || trimmed.Contains("@"))
                continue;

            // Check if it's a valid business name
            if (IsValidBusinessName(trimmed))
            {
                // Higher confidence for lines near the top
                float positionMultiplier = lineIndex <= 5 ? 1.0f : 0.85f;

                result.Value = trimmed;
                result.PatternUsed = "Priority4_FirstLine";
                result.Factors.PatternPriority = 0.7f * positionMultiplier;
                result.Factors.PositionScore = positionMultiplier;
                result.Confidence = result.Factors.CalculateOverallConfidence();
                return result;
            }
        }

        return result;
    }

    protected override float GetPositionConfidenceForField(float position)
    {
        // Business name should appear in top 20% of document
        return position < 0.2f ? 1.0f : 0.7f;
    }

    private bool IsValidBusinessName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Length validation
        if (name.Length < 3 || name.Length > 80)
            return false;

        // Not just numbers
        if (Regex.IsMatch(name, @"^\d+$"))
            return false;

        // Not mostly numbers (more than 50% digits)
        int digitCount = name.Count(char.IsDigit);
        if (digitCount > name.Length * 0.5)
            return false;

        // Not a URL or email
        if (name.Contains("http://") || name.Contains("https://") ||
            name.Contains("www.") || name.Contains("@"))
            return false;

        return true;
    }

    private bool ContainsExcludeKeywords(string text)
    {
        return ExcludeKeywords.Any(keyword =>
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
