using System.Text;
using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Normalizers;

public class TextNormalizer
{
    private static readonly Regex MultipleSpacesPattern = new(@"[ \t]+", RegexOptions.Compiled);
    private static readonly Regex MultipleNewlinesPattern = new(@"\n{3,}", RegexOptions.Compiled);

    public string Normalize(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        string normalized = rawText;

        // 1. Unicode normalization (combine diacritics)
        normalized = normalized.Normalize(NormalizationForm.FormC);

        // 2. Convert smart quotes to ASCII
        normalized = normalized
            .Replace("\u201C", "\"").Replace("\u201D", "\"") // ""
            .Replace("\u2018", "'").Replace("\u2019", "'")   // ''
            .Replace("\u05F4", "\"").Replace("\u05F3", "'"); // Hebrew ״׳

        // 3. Normalize Hebrew punctuation
        normalized = normalized
            .Replace("\u05BE", "-")  // Hebrew hyphen (maqaf) → ASCII hyphen
            .Replace("\u200E", "")   // Remove LTR mark
            .Replace("\u200F", "");  // Remove RTL mark

        // 4. Normalize whitespace (preserve newlines)
        normalized = MultipleSpacesPattern.Replace(normalized, " ");
        normalized = MultipleNewlinesPattern.Replace(normalized, "\n\n");

        // 5. Fix common OCR errors in Hebrew
        normalized = FixHebrewOcrErrors(normalized);

        // 6. Fix common OCR errors in English
        normalized = FixEnglishOcrErrors(normalized);

        return normalized.Trim();
    }

    private string FixHebrewOcrErrors(string text)
    {
        // Common Hebrew OCR errors
        text = Regex.Replace(text, @"\bמ\s*ע\s*מ\b", "מע\"מ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bח\s*פ\b", "ח.פ.", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bע\s*מ\b", "ע.מ.", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bבע\s*מ\b", "בע\"מ", RegexOptions.IgnoreCase);

        // Fix spacing in common compound words
        text = Regex.Replace(text, @"סה\s*[""״]\s*כ", "סה\"כ", RegexOptions.IgnoreCase);

        return text;
    }

    private string FixEnglishOcrErrors(string text)
    {
        // Fix common spacing issues in acronyms
        text = Regex.Replace(text, @"\bV\s*A\s*T\b", "VAT", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bG\s*S\s*T\b", "GST", RegexOptions.IgnoreCase);

        // Note: We don't fix O→0 or l→I globally as they're context-dependent
        // These should be handled in specific extractors if needed

        return text;
    }
}
