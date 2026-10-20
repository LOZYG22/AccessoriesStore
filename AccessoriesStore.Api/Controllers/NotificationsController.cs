using System.Security.Claims;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var items = await db.Notifications.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).Take(100)
            .Select(x => new { x.Id, x.Title, x.Message, x.Type, x.Link, x.CreatedAt, x.ReadAt })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.SuccessResponse(items));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var count = await db.Notifications.CountAsync(x => x.UserId == userId && x.ReadAt == null, ct);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> Read(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await db.Notifications.Where(x => x.Id == id && x.UserId == userId && x.ReadAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
        return Ok(ApiResponse<object>.SuccessResponse(null));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await db.Notifications.Where(x => x.UserId == userId && x.ReadAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
        return Ok(ApiResponse<object>.SuccessResponse(null));
    }
}
