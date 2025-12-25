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
        // Hebrew: ח.פ, ע.מ, ע.פ | English: Company ID, Tax ID, VAT ID
        var patterns = new[]
        {
            @"(?:ח\.פ\.|ע\.מ\.|ע\.פ\.)\s*[:\-]?\s*([\d\-]+)",
            @"(?:Company ID|Tax ID|VAT ID|Business ID)\s*[:\-]?\s*(\d+)",
            @"(?:ID)\s*[:\-]?\s*(\d{8,})" // At least 8 digits to avoid small numbers
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
        // First try to find explicit "Business Name:" pattern
        var explicitPatterns = new[]
        {
            @"(?:Business Name|Company Name|שם העסק)\s*[:\-]?\s*(.+)",
            @"(?:Name)\s*[:\-]?\s*(.+)"
        };

        foreach (var pattern in explicitPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var name = match.Groups[1].Value.Trim();
                // Make sure it's not a number or too long
                if (name.Length > 2 && name.Length < 100 && !Regex.IsMatch(name, @"^\d+$"))
                {
                    return name;
                }
            }
        }

        // Fallback: Try to find business name near top of document
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines.Take(15))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 3 && trimmed.Length < 100 &&
                !Regex.IsMatch(trimmed, @"^\d+$") && // Not just numbers
                !Regex.IsMatch(trimmed, @"^\$") && // Not starting with currency
                !trimmed.Contains("חשבונית", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("invoice", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("receipt", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("total", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("amount", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("VAT", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Contains("description", StringComparison.OrdinalIgnoreCase))
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
        // Only match amounts that have currency symbols or are near amount keywords
        var amountPattern = @"(?:₪|NIS|\$)\s*([\d,]+\.?\d*)";
        var amounts = new List<decimal>();

        foreach (Match match in Regex.Matches(text, amountPattern))
        {
            var valueStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                // Skip very large numbers that are likely IDs (> 100000)
                if (value < 100000)
                {
                    amounts.Add(value);
                }
            }
        }

        // Look for "Before VAT" amount first
        var beforeVatPatterns = new[]
        {
            @"(?:Amount|Sum|Total)?\s*\(?Before VAT\)?\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)",
            @"(?:לפני מע""מ)\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)"
        };

        foreach (var pattern in beforeVatPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var valueStr = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    beforeVat = value;
                    break;
                }
            }
        }

        // Look for VAT keywords
        var vatPatterns = new[]
        {
            @"VAT\s*\([\d]+%\)\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)",
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

        // Look for total keywords (Total Due, Total Amount, etc.)
        var totalPatterns = new[]
        {
            @"(?:Total Due|Total Amount|Grand Total)\s*[:\-]?\s*(?:₪|NIS|\$)?\s*([\d,]+\.?\d*)",
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
