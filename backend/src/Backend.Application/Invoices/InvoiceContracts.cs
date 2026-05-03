using Backend.Domain.Enums;

namespace Backend.Application.Invoices;

public sealed record GenerateInvoiceRequest(int OrderId, decimal TaxAmount);
public sealed record InvoiceDto(int Id, int OrderId, string InvoiceNumber, DateTime InvoiceDateUtc, decimal TotalAmount, InvoiceStatus Status);

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GenerateAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default);
}
