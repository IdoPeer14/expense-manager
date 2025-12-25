using System.Text.RegularExpressions;

namespace ExpenseManager.Api.Services.Extractors;

public abstract class BaseFieldExtractor<T> : IFieldExtractor<T>
{
    public abstract string FieldName { get; }

    public abstract ExtractionResult<T> Extract(string normalizedText);

    protected float GetPositionConfidence(Match match, string fullText)
    {
        if (match == null || !match.Success || string.IsNullOrEmpty(fullText))
            return 0.9f;

        float position = (float)match.Index / fullText.Length;
        return GetPositionConfidenceForField(position);
    }

    protected virtual float GetPositionConfidenceForField(float position)
    {
        // Default: no position bias
        return 0.9f;
    }

    protected bool IsNearKeyword(string text, int matchPosition, string[] keywords, int maxDistance = 100)
    {
        int startPos = Math.Max(0, matchPosition - maxDistance);
        int endPos = Math.Min(text.Length, matchPosition + maxDistance);
        int length = endPos - startPos;

        if (length <= 0) return false;

        string context = text.Substring(startPos, length);

        return keywords.Any(keyword =>
            context.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    protected string GetContextLines(string text, int position, int linesBefore = 2, int linesAfter = 2)
    {
        var lines = text.Split('\n');
        int currentLine = 0;
        int charCount = 0;

        // Find which line the position is on
        for (int i = 0; i < lines.Length; i++)
        {
            charCount += lines[i].Length + 1; // +1 for newline
            if (charCount >= position)
            {
                currentLine = i;
                break;
            }
        }

        int startLine = Math.Max(0, currentLine - linesBefore);
        int endLine = Math.Min(lines.Length - 1, currentLine + linesAfter);

        return string.Join("\n", lines.Skip(startLine).Take(endLine - startLine + 1));
    }
}
