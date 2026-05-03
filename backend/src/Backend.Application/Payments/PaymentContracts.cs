using Backend.Domain.Enums;

namespace Backend.Application.Payments;

public sealed record CreatePaymentRequest(int InvoiceId, decimal Amount, PaymentMethod Method, string? TransactionReference);
public sealed record PaymentDto(int Id, int InvoiceId, decimal Amount, PaymentMethod Method, PaymentStatus Status, DateTime PaymentDateUtc, string? TransactionReference);

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
