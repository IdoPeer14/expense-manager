using ExpenseManager.Api.Data;
using ExpenseManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : BaseAuthController
{
    private readonly AppDbContext _db;

    public ExpensesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(request.BusinessName))
        {
            return BadRequest(new { error = "Business name is required." });
        }

        var expense = new Expense
        {
            UserId = userId,
            DocumentId = request.DocumentId,
            BusinessName = request.BusinessName,
            BusinessId = request.BusinessId,
            InvoiceNumber = request.InvoiceNumber,
            ServiceDescription = request.ServiceDescription,
            TransactionDate = request.TransactionDate,
            AmountBeforeVat = request.AmountBeforeVat,
            AmountAfterVat = request.AmountAfterVat,
            VatAmount = request.VatAmount,
            Category = request.Category
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = expense.Id,
            vendorName = expense.BusinessName,
            date = expense.TransactionDate,
            totalAmount = expense.AmountAfterVat,
            invoiceNumber = expense.InvoiceNumber,
            category = expense.Category.ToString(),
            // Additional fields
            userId = expense.UserId,
            documentId = expense.DocumentId,
            businessId = expense.BusinessId,
            description = expense.ServiceDescription,
            amountBeforeVat = expense.AmountBeforeVat,
            vatAmount = expense.VatAmount,
            createdAt = expense.CreatedAt,
            updatedAt = expense.UpdatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] string? category,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] decimal? amountMin,
        [FromQuery] decimal? amountMax)
    {
        var userId = GetCurrentUserId();
        var query = _db.Expenses.Where(e => e.UserId == userId);

        // Server-side filtering
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ExpenseCategory>(category, true, out var categoryEnum))
        {
            query = query.Where(e => e.Category == categoryEnum);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(e => e.TransactionDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(e => e.TransactionDate <= dateTo.Value);
        }

        if (amountMin.HasValue)
        {
            query = query.Where(e => e.AmountAfterVat >= amountMin.Value);
        }

        if (amountMax.HasValue)
        {
            query = query.Where(e => e.AmountAfterVat <= amountMax.Value);
        }

        var expenses = await query
            .OrderByDescending(e => e.TransactionDate)
            .Select(e => new
            {
                id = e.Id,
                vendorName = e.BusinessName,
                date = e.TransactionDate,
                totalAmount = e.AmountAfterVat,
                invoiceNumber = e.InvoiceNumber,
                category = e.Category.ToString(),
                // Additional fields
                documentId = e.DocumentId,
                businessId = e.BusinessId,
                description = e.ServiceDescription,
                amountBeforeVat = e.AmountBeforeVat,
                vatAmount = e.VatAmount,
                createdAt = e.CreatedAt,
                updatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { data = expenses, pagination = new { totalCount = expenses.Count } });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var userId = GetCurrentUserId();
        var expense = await _db.Expenses.FindAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound(new { error = "Expense not found." });
        }

        if (request.Category.HasValue)
        {
            expense.Category = request.Category.Value;
        }

        if (request.TransactionDate.HasValue)
        {
            expense.TransactionDate = request.TransactionDate.Value;
        }

        if (request.AmountBeforeVat.HasValue)
        {
            expense.AmountBeforeVat = request.AmountBeforeVat.Value;
        }

        if (request.AmountAfterVat.HasValue)
        {
            expense.AmountAfterVat = request.AmountAfterVat.Value;
        }

        if (request.VatAmount.HasValue)
        {
            expense.VatAmount = request.VatAmount.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessName))
        {
            expense.BusinessName = request.BusinessName;
        }

        if (request.BusinessId != null)
        {
            expense.BusinessId = request.BusinessId;
        }

        if (request.InvoiceNumber != null)
        {
            expense.InvoiceNumber = request.InvoiceNumber;
        }

        if (request.ServiceDescription != null)
        {
            expense.ServiceDescription = request.ServiceDescription;
        }

        expense.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = expense.Id,
            category = expense.Category.ToString(),
            transactionDate = expense.TransactionDate,
            businessName = expense.BusinessName,
            updatedAt = expense.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var userId = GetCurrentUserId();
        var expense = await _db.Expenses.FindAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound(new { error = "Expense not found." });
        }

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Expense deleted successfully." });
    }
}

public record CreateExpenseRequest(
    Guid? DocumentId,
    string BusinessName,
    string? BusinessId,
    string? InvoiceNumber,
    string? ServiceDescription,
    DateTime TransactionDate,
    decimal AmountBeforeVat,
    decimal AmountAfterVat,
    decimal VatAmount,
    ExpenseCategory Category
);

public record UpdateExpenseRequest(
    ExpenseCategory? Category,
    DateTime? TransactionDate,
    decimal? AmountBeforeVat,
    decimal? AmountAfterVat,
    decimal? VatAmount,
    string? BusinessName,
    string? BusinessId,
    string? InvoiceNumber,
    string? ServiceDescription
);
