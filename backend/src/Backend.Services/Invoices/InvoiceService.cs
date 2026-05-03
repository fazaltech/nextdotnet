using Backend.Application.Invoices;
using Backend.Application.Persistence;

namespace Backend.Services.Invoices;

public sealed class InvoiceService(IInvoiceRepository repository) : IInvoiceService
{
    public Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> GenerateAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default) => repository.GenerateAsync(request, cancellationToken);
}
