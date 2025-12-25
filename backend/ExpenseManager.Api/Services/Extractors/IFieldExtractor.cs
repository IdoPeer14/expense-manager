namespace ExpenseManager.Api.Services.Extractors;

public interface IFieldExtractor<T>
{
    /// <summary>
    /// Extract a field from normalized OCR text
    /// </summary>
    /// <param name="normalizedText">Pre-normalized OCR text</param>
    /// <returns>Extraction result with confidence scoring</returns>
    ExtractionResult<T> Extract(string normalizedText);

    /// <summary>
    /// Field name for logging/debugging
    /// </summary>
    string FieldName { get; }
}
