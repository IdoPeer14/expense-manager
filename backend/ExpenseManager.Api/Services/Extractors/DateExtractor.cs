using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public class DateExtractor : BaseFieldExtractor<DateTime>
{
    // Priority 1: Explicit date label
    private static readonly Regex Priority1Pattern = new(
        @"(?:תאריך|date)\s*[:\-]?\s*(\d{1,2})[\/\.\-](\d{1,2})[\/\.\-](\d{2,4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 2: Common Israeli format (DD/MM/YYYY or DD.MM.YYYY)
    private static readonly Regex Priority2Pattern = new(
        @"\b(\d{1,2})[\/\.](\d{1,2})[\/\.](\d{4})\b",
        RegexOptions.Compiled
    );

    // Priority 3: ISO format (YYYY-MM-DD)
    private static readonly Regex Priority3Pattern = new(
        @"\b(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})\b",
        RegexOptions.Compiled
    );

    // Priority 4: Compact format (DDMMYYYY)
    private static readonly Regex Priority4Pattern = new(
        @"\b(\d{2})(\d{2})(\d{4})\b",
        RegexOptions.Compiled
    );

    // Priority 5: Month name format (e.g., "December 24, 2025", "24 December 2025")
    private static readonly Regex Priority5Pattern = new(
        @"\b(?:January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\s+(\d{1,2})\s*,?\s*(\d{4})\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Priority 6: Day first with month name (e.g., "24 December 2025")
    private static readonly Regex Priority6Pattern = new(
        @"\b(\d{1,2})\s+(?:January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\s+(\d{4})\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Dictionary<string, int> MonthNames = new(StringComparer.OrdinalIgnoreCase)
    {
        {"january", 1}, {"jan", 1},
        {"february", 2}, {"feb", 2},
        {"march", 3}, {"mar", 3},
        {"april", 4}, {"apr", 4},
        {"may", 5},
        {"june", 6}, {"jun", 6},
        {"july", 7}, {"jul", 7},
        {"august", 8}, {"aug", 8},
        {"september", 9}, {"sep", 9}, {"sept", 9},
        {"october", 10}, {"oct", 10},
        {"november", 11}, {"nov", 11},
        {"december", 12}, {"dec", 12}
    };

    public override string FieldName => "TransactionDate";

    public override ExtractionResult<DateTime> Extract(string normalizedText)
    {
        var result = new ExtractionResult<DateTime>
        {
            Confidence = 0.0f
        };

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        // Try patterns in priority order
        var attempts = new[]
        {
            (Pattern: Priority1Pattern, Name: "Priority1_ExplicitLabel", Confidence: 1.0f, IsISO: false, HasMonthName: false),
            (Pattern: Priority5Pattern, Name: "Priority5_MonthNameFirst", Confidence: 0.98f, IsISO: false, HasMonthName: true),
            (Pattern: Priority6Pattern, Name: "Priority6_DayMonthName", Confidence: 0.98f, IsISO: false, HasMonthName: true),
            (Pattern: Priority2Pattern, Name: "Priority2_IsraeliFormat", Confidence: 0.9f, IsISO: false, HasMonthName: false),
            (Pattern: Priority3Pattern, Name: "Priority3_ISOFormat", Confidence: 1.0f, IsISO: true, HasMonthName: false),
            (Pattern: Priority4Pattern, Name: "Priority4_CompactFormat", Confidence: 0.8f, IsISO: false, HasMonthName: false)
        };

        foreach (var attempt in attempts)
        {
            var match = attempt.Pattern.Match(normalizedText);

            if (!match.Success)
                continue;

            DateTime? parsedDate;

            if (attempt.HasMonthName)
            {
                // Extract month name from the match
                string monthName = ExtractMonthName(match.Value);
                if (attempt.Name == "Priority5_MonthNameFirst")
                {
                    // Format: "December 24, 2025"
                    parsedDate = ParseDateWithMonthName(
                        monthName,
                        match.Groups[1].Value,
                        match.Groups[2].Value
                    );
                }
                else
                {
                    // Format: "24 December 2025"
                    parsedDate = ParseDateWithMonthName(
                        monthName,
                        match.Groups[1].Value,
                        match.Groups[2].Value
                    );
                }
            }
            else
            {
                parsedDate = ParseDate(
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    attempt.IsISO
                );
            }

            if (parsedDate.HasValue && IsValidInvoiceDate(parsedDate.Value))
            {
                result.Value = parsedDate.Value;
                result.PatternUsed = attempt.Name;
                result.Factors.PatternPriority = attempt.Confidence;
                result.Factors.PositionScore = GetPositionConfidence(match, normalizedText);
                result.Confidence = result.Factors.CalculateOverallConfidence();

                return result;
            }
        }

        return result;
    }

    protected override float GetPositionConfidenceForField(float position)
    {
        // Date typically appears in top 40% of document
        return position < 0.4f ? 1.0f : 0.85f;
    }

    private DateTime? ParseDate(string part1, string part2, string part3, bool isISO)
    {
        try
        {
            int firstPart = int.Parse(part1);
            int secondPart = int.Parse(part2);
            int thirdPart = int.Parse(part3);

            int year, month, day;

            if (isISO)
            {
                // ISO format: YYYY-MM-DD
                year = firstPart;
                month = secondPart;
                day = thirdPart;
            }
            else
            {
                // Normalize year (handle 2-digit years)
                year = thirdPart < 100 ? thirdPart + 2000 : thirdPart;

                // Detect format ambiguity (DD/MM vs MM/DD)
                if (firstPart > 12 && secondPart <= 12)
                {
                    // firstPart must be day
                    day = firstPart;
                    month = secondPart;
                }
                else if (secondPart > 12 && firstPart <= 12)
                {
                    // secondPart must be day
                    day = secondPart;
                    month = firstPart;
                }
                else
                {
                    // Prefer Israeli format (DD/MM) unless context suggests otherwise
                    day = firstPart;
                    month = secondPart;
                }

                // Additional validation: if year appears first, swap
                if (firstPart > 31 && firstPart > 1900)
                {
                    year = firstPart;
                    month = secondPart;
                    day = thirdPart;
                }
            }

            // Validate ranges
            if (year < 1900 || year > 2100)
                return null;

            if (month < 1 || month > 12)
                return null;

            if (day < 1 || day > 31)
                return null;

            // Try to create the date
            return new DateTime(year, month, day);
        }
        catch
        {
            return null;
        }
    }

    private string ExtractMonthName(string text)
    {
        foreach (var monthName in MonthNames.Keys)
        {
            if (text.Contains(monthName, StringComparison.OrdinalIgnoreCase))
            {
                return monthName;
            }
        }
        return string.Empty;
    }

    private DateTime? ParseDateWithMonthName(string monthName, string dayOrYear1, string dayOrYear2)
    {
        try
        {
            if (!MonthNames.TryGetValue(monthName, out int month))
                return null;

            int day, year;

            // Determine which parameter is day and which is year
            int val1 = int.Parse(dayOrYear1);
            int val2 = int.Parse(dayOrYear2);

            if (val1 > 31)
            {
                // val1 is year
                year = val1;
                day = val2;
            }
            else if (val2 > 31)
            {
                // val2 is year
                year = val2;
                day = val1;
            }
            else
            {
                // Assume the larger value is year if both are small
                if (val2 > 2000 || val2 > val1)
                {
                    year = val2;
                    day = val1;
                }
                else
                {
                    year = val1;
                    day = val2;
                }
            }

            // Validate ranges
            if (year < 1900 || year > 2100)
                return null;

            if (day < 1 || day > 31)
                return null;

            if (month < 1 || month > 12)
                return null;

            // Try to create the date
            return new DateTime(year, month, day);
        }
        catch
        {
            return null;
        }
    }

    private bool IsValidInvoiceDate(DateTime date)
    {
        DateTime minDate = new DateTime(2000, 1, 1);
        DateTime maxDate = DateTime.Now.AddYears(1); // Allow 1 year in future

        return date >= minDate && date <= maxDate;
    }
}
