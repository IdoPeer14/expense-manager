namespace ExpenseManager.Api.Services.Extractors;

public class ExtractionResult<T>
{
    public T? Value { get; set; }
    public float Confidence { get; set; }
    public string PatternUsed { get; set; } = string.Empty;
    public ConfidenceFactors Factors { get; set; } = new();
    public bool IsSuccess => Confidence >= 0.4f && Value != null;
}

public class ConfidenceFactors
{
    public float PatternPriority { get; set; } = 1.0f;      // 0.0 - 1.0
    public float ContextValidation { get; set; } = 1.0f;    // 0.0 - 1.0
    public float PositionScore { get; set; } = 1.0f;        // 0.0 - 1.0
    public float CrossFieldValidation { get; set; } = 1.0f; // 0.0 - 1.0

    public float CalculateOverallConfidence()
    {
        return (PatternPriority * 0.4f +
                ContextValidation * 0.3f +
                PositionScore * 0.2f +
                CrossFieldValidation * 0.1f);
    }
}
