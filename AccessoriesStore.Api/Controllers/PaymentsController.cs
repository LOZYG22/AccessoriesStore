using System.Security.Claims;
using AccessoriesStore.Application.Abstractions.Payments;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;

    public PaymentsController(IPaymentService paymentService, ApplicationDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> CreatePayment(
        CreatePaymentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _paymentService.CreatePaymentAsync(
            userId,
            request);

        return Ok(
            ApiResponse<PaymentResponse>.SuccessResponse(
                result,
                "Payment created successfully."));
    }

    [HttpGet("orders/{orderId:int}/attempts")]
    public async Task<ActionResult<ApiResponse<object>>> GetAttempts(int orderId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        var ownsOrder = await _context.Orders.AnyAsync(x => x.Id == orderId && (isAdmin || x.UserId == userId), cancellationToken);
        if (!ownsOrder) return NotFound(ApiResponse<object>.Failure("Order not found."));

        var attempts = await _context.PaymentAttempts.AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Status, x.AmountCents, x.RefundedAmountCents, x.CreatedAt, x.UpdatedAt })
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(attempts));
    }
}
