using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Payments;
using AccessoriesStore.Infrastructure.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/payments/paymob")]
public class PaymobWebhookController : ControllerBase
{
    private readonly PaymobWebhookService _webhookService;

    public PaymobWebhookController(
        PaymobWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<ActionResult<ApiResponse<object>>> HandleWebhook(
        PaymobTransactionCallbackRequest request,
        [FromQuery] string hmac)
    {
        await _webhookService.ProcessTransactionAsync(
            request,
            hmac);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Webhook processed successfully."));
    }
}