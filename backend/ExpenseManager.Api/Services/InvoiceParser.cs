using System.Globalization;
using System.Text.RegularExpressions;
using ExpenseManager.Api.Models;

namespace ExpenseManager.Api.Services;

public class InvoiceParser
{
    public ParsedInvoiceData ParseInvoiceFields(string ocrText)
    {
        var result = new ParsedInvoiceData();

        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return result;
        }

        result.TransactionDate = ExtractDate(ocrText);
        result.InvoiceNumber = ExtractInvoiceNumber(ocrText);
        result.BusinessId = ExtractBusinessId(ocrText);
        result.BusinessName = ExtractBusinessName(ocrText);

        var amounts = ExtractAmounts(ocrText);
        result.AmountBeforeVat = amounts.beforeVat;
        result.AmountAfterVat = amounts.afterVat;
        result.VatAmount = amounts.vat;

        result.ServiceDescription = ExtractServiceDescription(ocrText);

        return result;
    }

    private DateTime? ExtractDate(string text)
    {
        // Patterns: DD/MM/YYYY, DD.MM.YYYY, DD-MM-YYYY
        var datePatterns = new[]
        {
            @"(\d{1,2})[\/\.\-](\d{1,2})[\/\.\-](\d{4})",
            @"(\d{4})[\/\.\-](\d{1,2})[\/\.\-](\d{1,2})" // YYYY-MM-DD
        };

        foreach (var pattern in datePatterns)
        {
            var match = Regex.Match(text, pattern);
            if (match.Success)
            {
                try
                {
                    // Try DD/MM/YYYY format first
                    var day = int.Parse(match.Groups[1].Value);
                    var month = int.Parse(match.Groups[2].Value);
                    var year = int.Parse(match.Groups[3].Value);

                    // If year appears first, swap
                    if (year < 100)
                    {
                        var temp = year;
                        year = day;
                        day = temp;
                    }

                    if (day > 31)
                    {
                        var temp = day;
                        day = year;
                        year = temp;
                    }

                    return new DateTime(year, month, day);
                }
                catch
                {
                    continue;
                }
            }
        }

        return null;
    }

    private string? ExtractInvoiceNumber(string text)
    {
        // Hebrew: חשבונית, מספר | English: Invoice, Number, #
        var patterns = new[]
        {
            @"(?:חשבונית|invoice)\s*(?:מס'|מספר|#|num|no\.?|number)?\s*[:\-]?\s*(\d+)",
            @"(?:מס'|מספר|#|no\.?)\s*[:\-]?\s*(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }

    private string? ExtractBusinessId(string text)
    {
        // Hebrew: ח.פ, ע.מ, ע.פ | English: VAT, Tax ID
        var patterns = new[]
        {
            @"(?:ח\.פ\.|ע\.מ\.|ע\.פ\.)\s*[:\-]?\s*([\d\-]+)",
            @"(?:VAT|Tax ID)\s*[:\-]?\s*([\d\-]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Replace("-", "");
            }
        }

        return null;
    }

    private string? ExtractBusinessName(string text)
    {
        // Try to find business name near top of document
        // Look for lines before invoice number or date
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines.Take(10))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 3 && trimmed.Length < 100 &&
                !Regex.IsMatch(trimmed, @"^\d+$") && // Not just numbers
                !trimmed.Contains("חשבונית", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("invoice", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }

        return null;
    }

    private (decimal? beforeVat, decimal? afterVat, decimal? vat) ExtractAmounts(string text)
    {
        decimal? beforeVat = null;
        decimal? afterVat = null;
        decimal? vat = null;

        // Extract all monetary values (supports ₪, $, NIS, comma/dot separators)
        var amountPattern = @"(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)";
        var amounts = new List<decimal>();

        foreach (Match match in Regex.Matches(text, amountPattern))
        {
            var valueStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                amounts.Add(value);
            }
        }

        // Look for VAT keywords
        var vatPatterns = new[]
        {
            @"(?:מע""מ|VAT|tax)\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)",
            @"([\d,]+\.?\d*)\s*(?:מע""מ|VAT)"
        };

        foreach (var pattern in vatPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var valueStr = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    vat = value;
                    break;
                }
            }
        }

        // Look for total keywords
        var totalPatterns = new[]
        {
            @"(?:סה""כ|total|sum|סכום)\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)",
            @"([\d,]+\.?\d*)\s*(?:סה""כ|total)"
        };

        foreach (var pattern in totalPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var valueStr = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    afterVat = value;
                    break;
                }
            }
        }

        // If we have VAT and total, calculate before VAT
        if (vat.HasValue && afterVat.HasValue)
        {
            beforeVat = afterVat.Value - vat.Value;
        }
        // Otherwise, use heuristics from extracted amounts
        else if (amounts.Count >= 2)
        {
            // Assume largest value is total, and calculate VAT as ~17%
            afterVat = amounts.Max();
            vat = afterVat.Value * 0.17m;
            beforeVat = afterVat.Value - vat.Value;
        }

        return (beforeVat, afterVat, vat);
    }

    private string? ExtractServiceDescription(string text)
    {
        // Look for lines that might describe services
        // This is a simple heuristic - can be improved
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 10 && trimmed.Length < 200 &&
                (trimmed.Contains("שירות") || trimmed.Contains("service") ||
                 trimmed.Contains("מוצר") || trimmed.Contains("product")))
            {
                return trimmed;
            }
        }

        return null;
    }
}
