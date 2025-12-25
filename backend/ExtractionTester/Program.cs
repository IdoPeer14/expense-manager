using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseManager.Api.Services;
using ExpenseManager.Api.Models;

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     REGEX-BASED INVOICE EXTRACTION ENGINE - TEST SUITE          ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configuration
var receiptsDir = args.Length > 0 ? args[0] : "../Receipts";
var outputJsonPath = "extraction_results.json";

if (!Directory.Exists(receiptsDir))
{
    Console.WriteLine($"❌ Receipts directory not found: {receiptsDir}");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run [receipts-directory]");
    Console.WriteLine("Example: dotnet run ../Receipts");
    Console.WriteLine();
    Console.WriteLine("Creating Receipts directory...");
    Directory.CreateDirectory(receiptsDir);
    Console.WriteLine($"✅ Created: {Path.GetFullPath(receiptsDir)}");
    Console.WriteLine();
    Console.WriteLine("Please add PDF or image files to this directory and run again.");
    return 1;
}

// Find receipt files
var supportedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".tif", ".tiff" };
var receiptFiles = Directory.GetFiles(receiptsDir)
    .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
    .OrderBy(f => f)
    .ToArray();

if (receiptFiles.Length == 0)
{
    Console.WriteLine($"📂 Receipts directory: {Path.GetFullPath(receiptsDir)}");
    Console.WriteLine($"❌ No receipt files found!");
    Console.WriteLine();
    Console.WriteLine($"Supported formats: {string.Join(", ", supportedExtensions)}");
    Console.WriteLine();
    Console.WriteLine("Please add some receipt files and run again.");
    return 1;
}

Console.WriteLine($"📂 Receipts directory: {Path.GetFullPath(receiptsDir)}");
Console.WriteLine($"📄 Found {receiptFiles.Length} file(s) to process");
Console.WriteLine();

// Check if tesseract is available
Console.WriteLine("🔍 Checking Tesseract OCR...");
var tesseractPath = GetTesseractPath();

if (tesseractPath == null)
{
    Console.WriteLine("❌ Tesseract not found. Please install:");
    Console.WriteLine("   macOS: brew install tesseract tesseract-lang");
    Console.WriteLine("   Ubuntu: sudo apt install tesseract-ocr tesseract-ocr-heb tesseract-ocr-eng");
    return 1;
}

Console.WriteLine($"✅ Using tesseract: {tesseractPath}");
Console.WriteLine();

// Initialize extraction engine
var parser = new InvoiceParser();
var results = new List<ExtractionTestResult>();

// Process each file
for (int i = 0; i < receiptFiles.Length; i++)
{
    var file = receiptFiles[i];
    var fileName = Path.GetFileName(file);

    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"📄 [{i + 1}/{receiptFiles.Length}] {fileName}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine();

    try
    {
        // Step 1: OCR
        Console.WriteLine("🔍 Running OCR (Hebrew + English)...");
        string ocrText;
        float ocrConfidence;

        var ext = Path.GetExtension(file).ToLowerInvariant();
        var isPdf = ext == ".pdf";

        if (isPdf)
        {
            Console.WriteLine("   ⚠️  PDF detected - Tesseract PDF support may be limited");
            Console.WriteLine("   💡 Tip: Convert PDFs to PNG/JPG for best results");
            Console.WriteLine();
        }

        try
        {
            (ocrText, ocrConfidence) = RunTesseractOCR(file);
        }
        catch (Exception ocrEx)
        {
            if (isPdf)
            {
                throw new Exception(
                    $"PDF OCR failed.\n" +
                    $"   Solution: Convert PDFs to images first using:\n" +
                    $"   cd backend && ./convert-pdfs-to-images.sh\n" +
                    $"   Original error: {ocrEx.Message}",
                    ocrEx);
            }
            throw;
        }

        Console.WriteLine($"   ✅ OCR completed (confidence: {ocrConfidence:P1})");
        Console.WriteLine($"   📝 Extracted {ocrText.Length} characters");
        Console.WriteLine();

        // Step 2: Extract fields
        Console.WriteLine("⚙️  Running extraction pipeline...");
        var extractionResult = parser.ParseInvoiceFields(ocrText);
        Console.WriteLine($"   ✅ Extraction completed (overall confidence: {extractionResult.OverallConfidence:P1})");
        Console.WriteLine();

        // Step 3: Display results
        DisplayResults(extractionResult);

        // Save for JSON output
        results.Add(new ExtractionTestResult
        {
            FileName = fileName,
            OcrConfidence = ocrConfidence,
            OcrTextLength = ocrText.Length,
            OcrText = ocrText,
            ExtractionResult = extractionResult
        });

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner error: {ex.InnerException.Message}");
        }
        Console.WriteLine();
        results.Add(new ExtractionTestResult
        {
            FileName = fileName,
            Error = ex.Message
        });
    }
}

// Summary
Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                          SUMMARY                                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var successCount = results.Count(r => r.Error == null);
var failCount = results.Count(r => r.Error != null);

Console.WriteLine($"✅ Successful: {successCount}/{results.Count}");
Console.WriteLine($"❌ Failed: {failCount}/{results.Count}");
Console.WriteLine();

if (successCount > 0)
{
    var avgConfidence = results
        .Where(r => r.ExtractionResult != null)
        .Average(r => r.ExtractionResult!.OverallConfidence);

    Console.WriteLine($"📊 Average extraction confidence: {avgConfidence:P1}");
    Console.WriteLine();
}

// Save JSON output
Console.WriteLine($"💾 Saving results to {outputJsonPath}...");
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
File.WriteAllText(outputJsonPath, JsonSerializer.Serialize(results, jsonOptions));
Console.WriteLine($"   ✅ Saved to {Path.GetFullPath(outputJsonPath)}");
Console.WriteLine();

Console.WriteLine("✨ Done!");
return 0;

// Helper methods
static void DisplayResults(ParsedInvoiceData data)
{
    Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
    Console.WriteLine("│                     EXTRACTION RESULTS                          │");
    Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
    Console.WriteLine();

    PrintField("Document Type", data.DocumentType.ToString(), data.DocumentTypeConfidence);
    PrintField("Invoice Number", data.InvoiceNumber, data.InvoiceNumberConfidence);
    PrintField("Transaction Date", data.TransactionDate?.ToString("yyyy-MM-dd"), data.TransactionDateConfidence);
    PrintField("Business Name", data.BusinessName, data.BusinessNameConfidence);
    PrintField("Business ID", data.BusinessId, data.BusinessIdConfidence);
    Console.WriteLine();
    PrintField("Amount (Before VAT)", FormatCurrency(data.AmountBeforeVat), data.AmountBeforeVatConfidence);
    PrintField("VAT Amount", FormatCurrency(data.VatAmount), data.VatAmountConfidence);
    PrintField("Amount (After VAT)", FormatCurrency(data.AmountAfterVat), data.AmountAfterVatConfidence);
    Console.WriteLine();

    if (!string.IsNullOrEmpty(data.ReferenceNumber))
    {
        PrintField($"Reference ({data.ReferenceType})", data.ReferenceNumber, data.ReferenceNumberConfidence);
        Console.WriteLine();
    }

    Console.WriteLine($"📈 Overall Confidence: {GetConfidenceBar(data.OverallConfidence)} {data.OverallConfidence:P1}");
}

static void PrintField(string label, string? value, float confidence)
{
    var icon = GetConfidenceIcon(confidence);
    var valueDisplay = string.IsNullOrEmpty(value) ? "—" : value;
    var confidenceDisplay = confidence > 0 ? $"({confidence:P0})" : "";

    Console.WriteLine($"  {icon} {label,-20} {valueDisplay,-30} {confidenceDisplay}");
}

static string? FormatCurrency(decimal? amount)
{
    return amount.HasValue ? $"₪{amount.Value:N2}" : null;
}

static string GetConfidenceIcon(float confidence)
{
    if (confidence >= 0.95f) return "🟢";
    if (confidence >= 0.8f) return "🟡";
    if (confidence >= 0.6f) return "🟠";
    if (confidence > 0) return "🔴";
    return "⚫";
}

static string GetConfidenceBar(float confidence)
{
    var barLength = 20;
    var filled = (int)(confidence * barLength);
    var bar = new string('█', filled) + new string('░', barLength - filled);
    return $"[{bar}]";
}

static string? GetTesseractPath()
{
    var possiblePaths = new[] { "tesseract", "/opt/homebrew/bin/tesseract", "/usr/local/bin/tesseract", "/usr/bin/tesseract" };

    foreach (var path in possiblePaths)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                process.WaitForExit(1000);
                if (process.ExitCode == 0)
                {
                    return path;
                }
            }
        }
        catch { }
    }

    return null;
}

static (string text, float confidence) RunTesseractOCR(string imagePath)
{
    var outputBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var outputFile = outputBase + ".txt";

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = GetTesseractPath() ?? "tesseract",
            Arguments = $"\"{imagePath}\" \"{outputBase}\" -l heb+eng",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new Exception("Failed to start tesseract process");
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Tesseract failed: {error}");
        }

        if (!File.Exists(outputFile))
        {
            throw new Exception($"Tesseract output file not found: {outputFile}");
        }

        var text = File.ReadAllText(outputFile);

        // Estimate confidence (tesseract CLI doesn't provide this easily)
        // We'll just use a default value
        return (text, 0.85f);
    }
    finally
    {
        // Clean up temp files
        try
        {
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
        catch { }
    }
}

// Result classes
class ExtractionTestResult
{
    public string FileName { get; set; } = "";
    public float? OcrConfidence { get; set; }
    public int? OcrTextLength { get; set; }
    public string? OcrText { get; set; }
    public ParsedInvoiceData? ExtractionResult { get; set; }
    public string? Error { get; set; }
}
