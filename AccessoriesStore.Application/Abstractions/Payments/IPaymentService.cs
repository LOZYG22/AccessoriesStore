using AccessoriesStore.Application.DTOs.Payments;

namespace AccessoriesStore.Application.Abstractions.Payments
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(
        string userId,
        CreatePaymentRequest request);
    }
}
