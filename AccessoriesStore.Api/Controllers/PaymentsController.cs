using System.Security.Claims;
using AccessoriesStore.Application.Abstractions.Payments;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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
}