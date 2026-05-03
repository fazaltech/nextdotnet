using Backend.Application.Invoices;

namespace Backend.Application.Persistence;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GenerateAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default);
}
