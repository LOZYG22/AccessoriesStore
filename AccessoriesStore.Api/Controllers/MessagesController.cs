using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Infrastructure.Identity;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController(ApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    public sealed record NewConversation([Required, StringLength(120, MinimumLength = 3)] string Subject,
        [Required, StringLength(4000, MinimumLength = 1)] string Body);
    public sealed record NewMessage([Required, StringLength(4000, MinimumLength = 1)] string Body);

    [HttpGet("conversations")]
    public Task<IActionResult> CustomerConversations(CancellationToken ct) => List(false, ct);

    [HttpGet("admin/conversations")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> AdminConversations(CancellationToken ct) => List(true, ct);

    private async Task<IActionResult> List(bool admin, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var conversations = db.SupportConversations.AsNoTracking();
        if (!admin) conversations = conversations.Where(x => x.CustomerId == userId);
        var data = await conversations.OrderByDescending(x => x.UpdatedAt).Take(200)
            .Select(x => new
            {
                x.Id, x.Subject, x.Status, x.UpdatedAt,
                CustomerId = x.CustomerId,
                UnreadCount = x.Messages.Count(m => admin ? !m.IsAdmin && !m.IsReadByAdmin : m.IsAdmin && !m.IsReadByCustomer),
                LastMessage = x.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Body).FirstOrDefault()
            }).ToListAsync(ct);
        var customers = await users.Users.Where(u => data.Select(x => x.CustomerId).Contains(u.Id))
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), u.Email }).ToDictionaryAsync(x => x.Id, ct);
        var result = data.Select(x => new { x.Id, x.Subject, x.Status, x.UpdatedAt, x.UnreadCount, x.LastMessage,
            CustomerName = customers.GetValueOrDefault(x.CustomerId)?.Name ?? "Customer",
            CustomerEmail = customers.GetValueOrDefault(x.CustomerId)?.Email ?? "" }).ToList();
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> Create(NewConversation request, CancellationToken ct)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await users.FindByIdAsync(customerId);
        var name = user is null ? "Customer" : $"{user.FirstName} {user.LastName}".Trim();
        var conversation = new SupportConversation { CustomerId = customerId, Subject = request.Subject.Trim() };
        conversation.Messages.Add(new SupportMessage { SenderId = customerId, SenderName = name, Body = request.Body.Trim(), IsReadByCustomer = true });
        db.SupportConversations.Add(conversation);
        await db.SaveChangesAsync(ct);
        var adminIds = await users.GetUsersInRoleAsync("Admin");
        foreach (var admin in adminIds)
            db.Notifications.Add(new StoreNotification { UserId = admin.Id, Title = "New customer message", Message = request.Subject.Trim(), Type = "Message", Link = "/admin/messages" });
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { conversation.Id, conversation.Subject, conversation.Status, conversation.UpdatedAt,
            CustomerName = name, CustomerEmail = user?.Email ?? "", UnreadCount = 0, LastMessage = request.Body.Trim() }));
    }

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        var conversation = await db.SupportConversations.FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.CustomerId == userId), ct);
        if (conversation is null) return NotFound(ApiResponse<object>.Failure("Conversation not found."));
        if (isAdmin) await db.SupportMessages.Where(x => x.ConversationId == id && !x.IsAdmin && !x.IsReadByAdmin).ExecuteUpdateAsync(x => x.SetProperty(m => m.IsReadByAdmin, true), ct);
        else await db.SupportMessages.Where(x => x.ConversationId == id && x.IsAdmin && !x.IsReadByCustomer).ExecuteUpdateAsync(x => x.SetProperty(m => m.IsReadByCustomer, true), ct);
        var messages = await db.SupportMessages.AsNoTracking().Where(x => x.ConversationId == id).OrderBy(x => x.CreatedAt)
            .Select(x => new { x.Id, x.ConversationId, x.SenderId, x.SenderName, x.IsAdmin, x.Body, x.CreatedAt }).ToListAsync(ct);
        return Ok(ApiResponse<object>.SuccessResponse(messages));
    }

    [HttpPost("conversations/{id:int}")]
    public async Task<IActionResult> Send(int id, NewMessage request, CancellationToken ct)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        var conversation = await db.SupportConversations.FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.CustomerId == senderId), ct);
        if (conversation is null) return NotFound(ApiResponse<object>.Failure("Conversation not found."));
        if (conversation.Status != "Open") return BadRequest(ApiResponse<object>.Failure("This conversation is closed."));
        var user = await users.FindByIdAsync(senderId);
        var senderName = isAdmin ? "Store Support" : $"{user?.FirstName} {user?.LastName}".Trim();
        var message = new SupportMessage { ConversationId = id, SenderId = senderId, SenderName = senderName,
            IsAdmin = isAdmin, IsReadByCustomer = !isAdmin, IsReadByAdmin = isAdmin, Body = request.Body.Trim() };
        conversation.UpdatedAt = DateTime.UtcNow;
        db.SupportMessages.Add(message);
        var notificationText = request.Body.Trim().Length > 160 ? request.Body.Trim()[..160] : request.Body.Trim();
        if (isAdmin)
            db.Notifications.Add(new StoreNotification { UserId = conversation.CustomerId, Title = "Reply from store support", Message = notificationText, Type = "Message", Link = "/account/messages" });
        else
            foreach (var admin in await users.GetUsersInRoleAsync("Admin"))
                db.Notifications.Add(new StoreNotification { UserId = admin.Id, Title = "New customer message", Message = notificationText, Type = "Message", Link = "/admin/messages" });
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { message.Id, message.ConversationId, message.SenderId, message.SenderName, message.IsAdmin, message.Body, message.CreatedAt }));
    }

    [HttpPut("admin/conversations/{id:int}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var changed = await db.SupportConversations.Where(x => x.Id == id && x.Status == "Open")
            .ExecuteUpdateAsync(x => x.SetProperty(c => c.Status, "Closed").SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct);
        return changed == 0 ? NotFound(ApiResponse<object>.Failure("Open conversation not found.")) : Ok(ApiResponse<object>.SuccessResponse(null));
    }
}
