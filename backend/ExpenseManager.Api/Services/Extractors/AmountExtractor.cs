using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public class MonetaryAmounts
{
    public ExtractionResult<decimal> Total { get; set; } = new() { Confidence = 0.0f };
    public ExtractionResult<decimal> VAT { get; set; } = new() { Confidence = 0.0f };
    public ExtractionResult<decimal> BeforeVAT { get; set; } = new() { Confidence = 0.0f };
}

public class AmountExtractor
{
    private const decimal ISRAEL_VAT_RATE = 0.17m;

    // Total Amount patterns
    private static readonly Regex TotalPattern1 = new(
        @"(?:Total\s*Due|Total\s*Amount|Grand\s*Total|Amount\s*Due)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex TotalPattern2 = new(
        @"(?:סה[""״']כ\s*לתשלום|סה[""״']כ|סכום\s*כולל)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex TotalPattern3 = new(
        @"(?:Total\s*(?:including|incl\.?)\s*VAT|כולל\s*מע[""״']מ)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Pattern for "Final Price:" format
    private static readonly Regex TotalPattern4 = new(
        @"(?:Final\s*Price|Total\s*Price|Price)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Generic currency amount pattern (lower confidence)
    private static readonly Regex TotalPattern5 = new(
        @"[\$₪€]\s*([\d,]+\.\d{2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Before VAT patterns
    private static readonly Regex BeforeVATPattern1 = new(
        @"(?:Amount\s*)?(?:Before|excl\.?|excluding)\s*VAT\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex BeforeVATPattern2 = new(
        @"(?:סכום\s*)?לפני\s*מע[""״']מ\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex BeforeVATPattern3 = new(
        @"(?:Subtotal|Sub-Total|סכום\s*ביניים)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex BeforeVATPattern4 = new(
        @"(?:Net\s*Amount|Net|נטו)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // VAT patterns
    private static readonly Regex VATPattern1 = new(
        @"VAT\s*\((\d+)%\)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex VATPattern2 = new(
        @"מע[""״']מ\s*(?:\((\d+)%\))?\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex VATPattern3 = new(
        @"(?:Tax|Sales\s*Tax|GST)\s*[:\-]?\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*([\d,]+\.?\d{0,2})\s*(?:USD|NIS|ILS|EUR)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex VATPattern4 = new(
        @"([\d,]+\.?\d{0,2})\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*מע[""״']מ",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex VATPattern5 = new(
        @"([\d,]+\.?\d{0,2})\s*(?:₪|NIS|\$|USD|ILS|EUR|€)?\s*VAT",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public MonetaryAmounts ExtractAmounts(string normalizedText)
    {
        var result = new MonetaryAmounts();

        if (string.IsNullOrWhiteSpace(normalizedText))
            return result;

        // Extract Total Amount
        result.Total = ExtractTotalAmount(normalizedText);

        // Extract VAT Amount
        result.VAT = ExtractVATAmount(normalizedText, result.Total.Value);

        // Extract Before VAT Amount
        result.BeforeVAT = ExtractBeforeVATAmount(normalizedText, result.Total.Value, result.VAT.Value);

        // Apply fallback logic
        ApplyFallbackLogic(ref result);

        // Validate consistency
        ValidateMonetaryConsistency(ref result);

        return result;
    }

    private ExtractionResult<decimal> ExtractTotalAmount(string text)
    {
        var patterns = new[]
        {
            (Pattern: TotalPattern1, Name: "Total_ExplicitDue", Confidence: 1.0f),
            (Pattern: TotalPattern2, Name: "Total_Hebrew", Confidence: 1.0f),
            (Pattern: TotalPattern3, Name: "Total_IncludingVAT", Confidence: 1.0f),
            (Pattern: TotalPattern4, Name: "Total_FinalPrice", Confidence: 0.95f),
            (Pattern: TotalPattern5, Name: "Total_GenericCurrency", Confidence: 0.7f)
        };

        foreach (var pattern in patterns)
        {
            var match = pattern.Pattern.Match(text);
            if (match.Success)
            {
                var amount = ParseAmount(match.Groups[1].Value);
                if (amount.HasValue && IsValidAmount(amount.Value))
                {
                    return new ExtractionResult<decimal>
                    {
                        Value = amount.Value,
                        PatternUsed = pattern.Name,
                        Factors = new ConfidenceFactors
                        {
                            PatternPriority = pattern.Confidence,
                            PositionScore = GetPositionScore(match.Index, text.Length, isTotal: true)
                        },
                        Confidence = pattern.Confidence * GetPositionScore(match.Index, text.Length, isTotal: true)
                    };
                }
            }
        }

        return new ExtractionResult<decimal> { Confidence = 0.0f };
    }

    private ExtractionResult<decimal> ExtractVATAmount(string text, decimal? totalAmount)
    {
        var patterns = new[]
        {
            (Pattern: VATPattern1, Name: "VAT_WithPercentage", Confidence: 1.0f, Group: 2),
            (Pattern: VATPattern2, Name: "VAT_Hebrew", Confidence: 1.0f, Group: 2),
            (Pattern: VATPattern3, Name: "VAT_GenericTax", Confidence: 0.95f, Group: 1),
            (Pattern: VATPattern4, Name: "VAT_HebrewReversed", Confidence: 0.9f, Group: 1),
            (Pattern: VATPattern5, Name: "VAT_EnglishReversed", Confidence: 0.9f, Group: 1)
        };

        foreach (var pattern in patterns)
        {
            var match = pattern.Pattern.Match(text);
            if (match.Success)
            {
                var amount = ParseAmount(match.Groups[pattern.Group].Value);
                if (amount.HasValue && IsValidAmount(amount.Value))
                {
                    float confidenceMultiplier = 1.0f;

                    // Validate against total if available
                    if (totalAmount.HasValue)
                    {
                        decimal expectedVAT = totalAmount.Value * ISRAEL_VAT_RATE / (1 + ISRAEL_VAT_RATE);
                        decimal deviation = Math.Abs(amount.Value - expectedVAT) / expectedVAT;

                        if (deviation > 0.05m) // More than 5% deviation
                        {
                            confidenceMultiplier = 0.8f;
                        }
                    }

                    return new ExtractionResult<decimal>
                    {
                        Value = amount.Value,
                        PatternUsed = pattern.Name,
                        Factors = new ConfidenceFactors
                        {
                            PatternPriority = pattern.Confidence * confidenceMultiplier,
                            PositionScore = 0.9f
                        },
                        Confidence = pattern.Confidence * confidenceMultiplier * 0.9f
                    };
                }
            }
        }

        return new ExtractionResult<decimal> { Confidence = 0.0f };
    }

    private ExtractionResult<decimal> ExtractBeforeVATAmount(string text, decimal? totalAmount, decimal? vatAmount)
    {
        var patterns = new[]
        {
            (Pattern: BeforeVATPattern1, Name: "BeforeVAT_Explicit", Confidence: 1.0f),
            (Pattern: BeforeVATPattern2, Name: "BeforeVAT_Hebrew", Confidence: 1.0f),
            (Pattern: BeforeVATPattern3, Name: "BeforeVAT_Subtotal", Confidence: 0.9f),
            (Pattern: BeforeVATPattern4, Name: "BeforeVAT_Net", Confidence: 0.9f)
        };

        foreach (var pattern in patterns)
        {
            var match = pattern.Pattern.Match(text);
            if (match.Success)
            {
                var amount = ParseAmount(match.Groups[1].Value);
                if (amount.HasValue && IsValidAmount(amount.Value))
                {
                    return new ExtractionResult<decimal>
                    {
                        Value = amount.Value,
                        PatternUsed = pattern.Name,
                        Factors = new ConfidenceFactors
                        {
                            PatternPriority = pattern.Confidence,
                            PositionScore = 0.9f
                        },
                        Confidence = pattern.Confidence * 0.9f
                    };
                }
            }
        }

        return new ExtractionResult<decimal> { Confidence = 0.0f };
    }

    private void ApplyFallbackLogic(ref MonetaryAmounts amounts)
    {
        // If we have total and VAT, calculate before VAT
        if (amounts.Total.IsSuccess && amounts.VAT.IsSuccess && !amounts.BeforeVAT.IsSuccess &&
            amounts.Total.Value != null && amounts.VAT.Value != null)
        {
            decimal total = (decimal)amounts.Total.Value;
            decimal vat = (decimal)amounts.VAT.Value;
            decimal beforeVAT = total - vat;

            if (IsValidAmount(beforeVAT))
            {
                amounts.BeforeVAT = new ExtractionResult<decimal>
                {
                    Value = beforeVAT,
                    PatternUsed = "Calculated_TotalMinusVAT",
                    Factors = new ConfidenceFactors
                    {
                        PatternPriority = Math.Min(amounts.Total.Confidence, amounts.VAT.Confidence) * 0.95f
                    },
                    Confidence = Math.Min(amounts.Total.Confidence, amounts.VAT.Confidence) * 0.95f
                };
            }
        }

        // If we have total but no VAT, calculate VAT
        if (amounts.Total.IsSuccess && !amounts.VAT.IsSuccess && amounts.Total.Value != null)
        {
            decimal total = (decimal)amounts.Total.Value;
            decimal vat = total * ISRAEL_VAT_RATE / (1 + ISRAEL_VAT_RATE);

            amounts.VAT = new ExtractionResult<decimal>
            {
                Value = vat,
                PatternUsed = "Calculated_17PercentVAT",
                Factors = new ConfidenceFactors
                {
                    PatternPriority = amounts.Total.Confidence * 0.7f
                },
                Confidence = amounts.Total.Confidence * 0.7f
            };

            amounts.BeforeVAT = new ExtractionResult<decimal>
            {
                Value = total - vat,
                PatternUsed = "Calculated_TotalMinusVAT",
                Factors = new ConfidenceFactors
                {
                    PatternPriority = amounts.Total.Confidence * 0.7f
                },
                Confidence = amounts.Total.Confidence * 0.7f
            };
        }
    }

    private void ValidateMonetaryConsistency(ref MonetaryAmounts amounts)
    {
        if (!amounts.Total.IsSuccess || !amounts.VAT.IsSuccess || !amounts.BeforeVAT.IsSuccess)
            return;

        if (amounts.Total.Value == null || amounts.VAT.Value == null || amounts.BeforeVAT.Value == null)
            return;

        decimal total = (decimal)amounts.Total.Value;
        decimal vat = (decimal)amounts.VAT.Value;
        decimal beforeVAT = (decimal)amounts.BeforeVAT.Value;

        decimal calculatedTotal = beforeVAT + vat;
        decimal deviation = Math.Abs(total - calculatedTotal) / total;

        // If deviation is more than 1%, lower confidence
        if (deviation > 0.01m)
        {
            amounts.Total.Factors.CrossFieldValidation = 0.8f;
            amounts.VAT.Factors.CrossFieldValidation = 0.8f;
            amounts.BeforeVAT.Factors.CrossFieldValidation = 0.8f;

            amounts.Total.Confidence = amounts.Total.Factors.CalculateOverallConfidence();
            amounts.VAT.Confidence = amounts.VAT.Factors.CalculateOverallConfidence();
            amounts.BeforeVAT.Confidence = amounts.BeforeVAT.Factors.CalculateOverallConfidence();
        }
    }

    private decimal? ParseAmount(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Remove commas
        string normalized = value.Replace(",", "");

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        return null;
    }

    private bool IsValidAmount(decimal amount)
    {
        return amount > 0 && amount < 1_000_000m;
    }

    private float GetPositionScore(int matchIndex, int textLength, bool isTotal)
    {
        float position = (float)matchIndex / textLength;

        if (isTotal)
        {
            // Total amount should appear in bottom 30% of document
            return position > 0.7f ? 1.0f : 0.8f;
        }

        return 0.9f;
    }
}
