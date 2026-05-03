using Backend.Application.Payments;
using Backend.Application.Persistence;

namespace Backend.Services.Payments;

public sealed class PaymentService(IPaymentRepository repository) : IPaymentService
{
    public Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
}
