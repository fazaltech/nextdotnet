using Backend.Application.Invoices;
using Backend.Application.Persistence;
using Backend.Domain.Enums;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class InvoiceRepository(IDbConnectionFactory connectionFactory) : IInvoiceRepository
{
    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, OrderId, InvoiceNumber, InvoiceDateUtc, TotalAmount, Status
FROM dbo.RmsInvoices
ORDER BY Id DESC;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<InvoiceDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, OrderId, InvoiceNumber, InvoiceDateUtc, TotalAmount, Status
FROM dbo.RmsInvoices
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<InvoiceDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> GenerateAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);

        const string existingInvoiceSql = "SELECT TOP 1 Id FROM dbo.RmsInvoices WHERE OrderId = @OrderId AND IsActive = 1;";
        var existingId = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(existingInvoiceSql, new { request.OrderId }, cancellationToken: cancellationToken));
        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        const string getOrderSql = "SELECT Subtotal FROM dbo.RmsSalesOrders WHERE Id = @OrderId;";
        var subtotal = await connection.ExecuteScalarAsync<decimal?>(new CommandDefinition(getOrderSql, new { request.OrderId }, cancellationToken: cancellationToken));
        if (!subtotal.HasValue)
        {
            throw new InvalidOperationException("Order was not found.");
        }

        var total = subtotal.Value + request.TaxAmount;

        const string updateOrderSql = @"
UPDATE dbo.RmsSalesOrders
SET TaxAmount = @TaxAmount,
    TotalAmount = @TotalAmount,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @OrderId;";

        await connection.ExecuteAsync(new CommandDefinition(updateOrderSql,
            new { request.OrderId, request.TaxAmount, TotalAmount = total }, cancellationToken: cancellationToken));

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.OrderId}";

        const string insertSql = @"
INSERT INTO dbo.RmsInvoices(OrderId, InvoiceNumber, InvoiceDateUtc, TotalAmount, Status, IsActive)
VALUES (@OrderId, @InvoiceNumber, SYSUTCDATETIME(), @TotalAmount, @Status, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(insertSql,
            new
            {
                request.OrderId,
                InvoiceNumber = invoiceNumber,
                TotalAmount = total,
                Status = (int)InvoiceStatus.Unpaid
            },
            cancellationToken: cancellationToken));
    }
}

