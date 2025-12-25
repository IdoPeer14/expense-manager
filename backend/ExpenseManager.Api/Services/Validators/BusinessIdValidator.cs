using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Validators;

public static class BusinessIdValidator
{
    /// <summary>
    /// Validates Israeli Business ID using Luhn-like checksum algorithm
    /// </summary>
    public static bool ValidateIsraeliBusinessID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        // Remove non-digits
        string normalized = Regex.Replace(id, @"[^\d]", "");

        // Israeli business IDs are 9 digits (some older: 8)
        if (normalized.Length != 9 && normalized.Length != 8)
            return false;

        if (normalized.Length == 8)
        {
            // Pad with leading zero for validation
            normalized = "0" + normalized;
        }

        // Calculate checksum
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = int.Parse(normalized[i].ToString());
            int multiplied = digit * ((i % 2) + 1);
            sum += multiplied > 9 ? multiplied - 9 : multiplied;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Normalizes business ID by removing all non-digit characters
    /// </summary>
    public static string NormalizeBusinessID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;

        return Regex.Replace(id, @"[^\d]", "");
    }
}
