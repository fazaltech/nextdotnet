using Backend.Application.Payments;

namespace Backend.Application.Persistence;

public interface IPaymentRepository
{
    Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
