using Backend.Application.Payments;
using Backend.Application.Persistence;
using Backend.Domain.Enums;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class PaymentRepository(IDbConnectionFactory connectionFactory) : IPaymentRepository
{
    public async Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, InvoiceId, Amount, Method, Status, PaymentDateUtc, TransactionReference
FROM dbo.RmsPayments
ORDER BY Id DESC;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PaymentDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, InvoiceId, Amount, Method, Status, PaymentDateUtc, TransactionReference
FROM dbo.RmsPayments
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PaymentDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);

        const string insertSql = @"
INSERT INTO dbo.RmsPayments(InvoiceId, Amount, Method, Status, PaymentDateUtc, TransactionReference, IsActive)
VALUES (@InvoiceId, @Amount, @Method, @Status, SYSUTCDATETIME(), @TransactionReference, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var paymentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(insertSql,
            new
            {
                request.InvoiceId,
                request.Amount,
                Method = (int)request.Method,
                Status = (int)PaymentStatus.Completed,
                request.TransactionReference
            },
            cancellationToken: cancellationToken));

        const string updateInvoiceStatusSql = @"
;WITH Paid AS
(
    SELECT InvoiceId, SUM(Amount) AS PaidAmount
    FROM dbo.RmsPayments
    WHERE InvoiceId = @InvoiceId AND Status = @CompletedStatus
    GROUP BY InvoiceId
)
UPDATE i
SET i.Status = CASE WHEN p.PaidAmount >= i.TotalAmount THEN @Paid ELSE @PartiallyPaid END,
    i.UpdatedAtUtc = SYSUTCDATETIME()
FROM dbo.RmsInvoices i
INNER JOIN Paid p ON p.InvoiceId = i.Id
WHERE i.Id = @InvoiceId;";

        await connection.ExecuteAsync(new CommandDefinition(updateInvoiceStatusSql,
            new
            {
                request.InvoiceId,
                CompletedStatus = (int)PaymentStatus.Completed,
                Paid = (int)InvoiceStatus.Paid,
                PartiallyPaid = (int)InvoiceStatus.PartiallyPaid
            },
            cancellationToken: cancellationToken));

        return paymentId;
    }
}

