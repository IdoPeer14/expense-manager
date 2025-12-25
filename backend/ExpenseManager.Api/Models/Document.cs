using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Api.Models;

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string OriginalFileName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string MimeType { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string StoragePath { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string? OcrText { get; set; }

    public string? ParsedJson { get; set; }

    public DocumentParseStatus ParseStatus { get; set; } = DocumentParseStatus.PENDING;

    public string? ParseError { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
