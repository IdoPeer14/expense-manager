using System.Text.RegularExpressions;

var text = "Invoice number JZMYWEKA-0003";
var pattern = @"(?:חשבונית|invoice|קבלה|receipt)\s*(?:מס[׳']|מספר|#|num|no\.?|number)?\s*[:\-]?\s*([A-Z0-9]+-\d+)";

var regex = new Regex(pattern, RegexOptions.IgnoreCase);
var match = regex.Match(text);

Console.WriteLine($"Match: {match.Success}");
if (match.Success)
{
    Console.WriteLine($"Captured: {match.Groups[1].Value}");
}
else
{
    Console.WriteLine("No match found");
}

// Also test the standalone pattern
var standalonePattern = @"\b([A-Z]{3,}[A-Z0-9]*-\d{3,})\b";
var standaloneRegex = new Regex(standalonePattern, RegexOptions.IgnoreCase);
var standaloneMatch = standaloneRegex.Match(text);

Console.WriteLine($"\nStandalone Match: {standaloneMatch.Success}");
if (standaloneMatch.Success)
{
    Console.WriteLine($"Captured: {standaloneMatch.Groups[1].Value}");
}
