namespace ExpenseManager.Api.Models;

public class ParsedInvoiceData
{
    public string? BusinessName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? AmountBeforeVat { get; set; }
    public decimal? AmountAfterVat { get; set; }
    public decimal? VatAmount { get; set; }
    public string? BusinessId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ServiceDescription { get; set; }
}
