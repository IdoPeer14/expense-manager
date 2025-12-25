using System;
using System.IO;
using Tesseract;

Console.WriteLine("========================================");
Console.WriteLine("Tesseract OCR Test");
Console.WriteLine("========================================");
Console.WriteLine();

// Get file path from command line or use default
var filePath = args.Length > 0 ? args[0] : "../../sample_receipt_en.pdf";

if (!File.Exists(filePath))
{
    Console.WriteLine($"❌ File not found: {filePath}");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run [file-path]");
    Console.WriteLine("Example: dotnet run ../../sample_receipt_en.pdf");
    return 1;
}

Console.WriteLine($"📄 Testing file: {filePath}");
Console.WriteLine();

// Try multiple possible tessdata locations
var possiblePaths = new[]
{
    "/opt/homebrew/share/tessdata",             // macOS Homebrew (Apple Silicon)
    "/usr/local/share/tessdata",                // macOS Homebrew (Intel)
    "/usr/share/tesseract-ocr/tessdata",        // Standard Debian/Ubuntu
    "/usr/share/tessdata",                      // Alternative Linux
    "/usr/share/tesseract-ocr/5.00/tessdata",   // Versioned Tesseract 5.x
    "/usr/share/tesseract-ocr/4.00/tessdata",   // Versioned Tesseract 4.x
};

string? tessdataPath = null;
Console.WriteLine("🔍 Searching for tessdata...");
foreach (var path in possiblePaths)
{
    Console.WriteLine($"   Checking: {path}");
    if (Directory.Exists(path))
    {
        tessdataPath = path;
        Console.WriteLine($"   ✅ Found!");
        break;
    }
}

if (tessdataPath == null)
{
    Console.WriteLine();
    Console.WriteLine($"❌ Tessdata directory not found. Searched:");
    foreach (var path in possiblePaths)
    {
        Console.WriteLine($"   - {path}");
    }
    return 1;
}

Console.WriteLine();
Console.WriteLine($"📁 Using tessdata: {tessdataPath}");
Console.WriteLine();

try
{
    // Check if Hebrew and English data files exist
    var hebFile = Path.Combine(tessdataPath, "heb.traineddata");
    var engFile = Path.Combine(tessdataPath, "eng.traineddata");

    Console.WriteLine("🔍 Checking language files...");
    Console.WriteLine($"   Hebrew: {(File.Exists(hebFile) ? "✅" : "❌")} {hebFile}");
    Console.WriteLine($"   English: {(File.Exists(engFile) ? "✅" : "❌")} {engFile}");
    Console.WriteLine();

    if (!File.Exists(hebFile) || !File.Exists(engFile))
    {
        Console.WriteLine("❌ Required language files not found!");
        Console.WriteLine();
        Console.WriteLine("Install with:");
        Console.WriteLine("  brew install tesseract-lang");
        return 1;
    }

    // Initialize Tesseract
    Console.WriteLine("🚀 Initializing Tesseract with Hebrew + English...");
    using var engine = new TesseractEngine(tessdataPath, "heb+eng", EngineMode.Default);
    Console.WriteLine("✅ Tesseract initialized successfully!");
    Console.WriteLine();

    // Process the image/PDF
    Console.WriteLine("📖 Processing document...");
    using var img = Pix.LoadFromFile(filePath);
    using var page = engine.Process(img);

    var text = page.GetText();
    var confidence = page.GetMeanConfidence();

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("OCR Results");
    Console.WriteLine("========================================");
    Console.WriteLine($"Confidence: {confidence:P2}");
    Console.WriteLine();
    Console.WriteLine("Extracted Text:");
    Console.WriteLine("----------------------------------------");
    Console.WriteLine(text);
    Console.WriteLine("----------------------------------------");
    Console.WriteLine();
    Console.WriteLine($"✅ Total characters extracted: {text.Length}");
    Console.WriteLine();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("❌ OCR FAILED");
    Console.WriteLine("========================================");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Stack trace:");
    Console.WriteLine(ex.StackTrace);
    Console.WriteLine();
    return 1;
}
