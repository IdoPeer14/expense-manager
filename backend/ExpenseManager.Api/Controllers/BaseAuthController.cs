using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.Api.Controllers;

public class BaseAuthController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token.");
        }

        return userId;
    }

    protected bool IsCurrentUser(Guid userId)
    {
        return GetCurrentUserId() == userId;
    }
}
