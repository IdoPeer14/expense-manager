using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Api.Models;

public class Expense
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required Guid UserId { get; set; }

    public Guid? DocumentId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string BusinessName { get; set; }

    [MaxLength(100)]
    public string? BusinessId { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    [MaxLength(1000)]
    public string? ServiceDescription { get; set; }

    [Required]
    public required DateTime TransactionDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal AmountBeforeVat { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal AmountAfterVat { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal VatAmount { get; set; }

    [Required]
    public required ExpenseCategory Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public Document? Document { get; set; }
}
