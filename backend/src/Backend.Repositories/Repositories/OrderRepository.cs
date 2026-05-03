using Backend.Application.Orders;
using Backend.Application.Persistence;
using Backend.Domain.Enums;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class OrderRepository(IDbConnectionFactory connectionFactory, ITableRepository tableRepository) : IOrderRepository
{
    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string orderSql = @"
SELECT so.Id,
       so.TableId,
       dt.TableNumber,
       so.CustomerId,
       c.FullName AS CustomerName,
       so.OrderDateUtc,
       so.Status,
       so.Subtotal,
       so.TaxAmount,
       so.TotalAmount
FROM dbo.RmsSalesOrders so
INNER JOIN dbo.RmsDiningTables dt ON dt.Id = so.TableId
LEFT JOIN dbo.RmsCustomers c ON c.Id = so.CustomerId
ORDER BY so.Id DESC;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var orderRows = (await connection.QueryAsync<OrderHeaderRow>(new CommandDefinition(orderSql, cancellationToken: cancellationToken))).ToArray();

        var result = new List<OrderDto>(orderRows.Length);
        foreach (var row in orderRows)
        {
            var items = await LoadItemsAsync(connection, row.Id, cancellationToken);
            result.Add(new OrderDto(
                row.Id,
                row.TableId,
                row.TableNumber,
                row.CustomerId,
                row.CustomerName,
                row.OrderDateUtc,
                row.Status,
                row.Subtotal,
                row.TaxAmount,
                row.TotalAmount,
                items));
        }

        return result;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string orderSql = @"
SELECT so.Id,
       so.TableId,
       dt.TableNumber,
       so.CustomerId,
       c.FullName AS CustomerName,
       so.OrderDateUtc,
       so.Status,
       so.Subtotal,
       so.TaxAmount,
       so.TotalAmount
FROM dbo.RmsSalesOrders so
INNER JOIN dbo.RmsDiningTables dt ON dt.Id = so.TableId
LEFT JOIN dbo.RmsCustomers c ON c.Id = so.CustomerId
WHERE so.Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<OrderHeaderRow>(new CommandDefinition(orderSql, new { Id = id }, cancellationToken: cancellationToken));
        if (row is null)
        {
            return null;
        }

        var items = await LoadItemsAsync(connection, row.Id, cancellationToken);
        return new OrderDto(
            row.Id,
            row.TableId,
            row.TableNumber,
            row.CustomerId,
            row.CustomerName,
            row.OrderDateUtc,
            row.Status,
            row.Subtotal,
            row.TaxAmount,
            row.TotalAmount,
            items);
    }

    public async Task<int> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var menuItemIds = request.Items.Select(x => x.MenuItemId).Distinct().ToArray();
            const string menuSql = @"
SELECT Id, Name, Price
FROM dbo.RmsMenuItems
WHERE Id IN @Ids
  AND IsActive = 1
  AND IsAvailable = 1;";

            var menuRows = (await connection.QueryAsync<MenuItemRow>(
                new CommandDefinition(menuSql, new { Ids = menuItemIds }, transaction, cancellationToken: cancellationToken))).ToDictionary(x => x.Id);

            if (menuRows.Count != menuItemIds.Length)
            {
                throw new InvalidOperationException("One or more menu items are invalid or unavailable.");
            }

            var subtotal = 0m;
            foreach (var item in request.Items)
            {
                subtotal += menuRows[item.MenuItemId].Price * item.Quantity;
            }

            const string createOrderSql = @"
INSERT INTO dbo.RmsSalesOrders(TableId, CustomerId, OrderDateUtc, Status, Subtotal, TaxAmount, TotalAmount, IsActive)
VALUES (@TableId, @CustomerId, SYSUTCDATETIME(), @Status, @Subtotal, 0, @Subtotal, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var orderId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                createOrderSql,
                new
                {
                    request.TableId,
                    request.CustomerId,
                    Status = (int)OrderStatus.Pending,
                    Subtotal = subtotal
                },
                transaction,
                cancellationToken: cancellationToken));

            const string createItemSql = @"
INSERT INTO dbo.RmsSalesOrderItems(OrderId, MenuItemId, Quantity, UnitPrice, LineTotal)
VALUES (@OrderId, @MenuItemId, @Quantity, @UnitPrice, @LineTotal);";

            foreach (var item in request.Items)
            {
                var unitPrice = menuRows[item.MenuItemId].Price;
                await connection.ExecuteAsync(new CommandDefinition(
                    createItemSql,
                    new
                    {
                        OrderId = orderId,
                        item.MenuItemId,
                        item.Quantity,
                        UnitPrice = unitPrice,
                        LineTotal = unitPrice * item.Quantity
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            const string occupySql = @"
UPDATE dbo.RmsDiningTables
SET IsOccupied = 1,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @TableId;";

            await connection.ExecuteAsync(new CommandDefinition(occupySql, new { request.TableId }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);

        const string updateSql = @"
UPDATE dbo.RmsSalesOrders
SET Status = @Status,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        var rows = await connection.ExecuteAsync(new CommandDefinition(updateSql, new { Id = id, Status = (int)status }, cancellationToken: cancellationToken));
        if (rows == 0)
        {
            return false;
        }

        if (status is OrderStatus.Served or OrderStatus.Cancelled)
        {
            const string getTableSql = "SELECT TableId FROM dbo.RmsSalesOrders WHERE Id = @Id;";
            var tableId = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(getTableSql, new { Id = id }, cancellationToken: cancellationToken));
            if (tableId.HasValue)
            {
                await tableRepository.SetOccupiedAsync(tableId.Value, false, cancellationToken);
            }
        }

        return true;
    }

    private static async Task<IReadOnlyList<OrderItemDto>> LoadItemsAsync(Microsoft.Data.SqlClient.SqlConnection connection, int orderId, CancellationToken cancellationToken)
    {
        const string itemsSql = @"
SELECT oi.Id,
       oi.MenuItemId,
       mi.Name AS MenuItemName,
       oi.Quantity,
       oi.UnitPrice,
       oi.LineTotal
FROM dbo.RmsSalesOrderItems oi
INNER JOIN dbo.RmsMenuItems mi ON mi.Id = oi.MenuItemId
WHERE oi.OrderId = @OrderId
ORDER BY oi.Id;";

        var items = await connection.QueryAsync<OrderItemDto>(new CommandDefinition(itemsSql, new { OrderId = orderId }, cancellationToken: cancellationToken));
        return items.ToArray();
    }

    private sealed class OrderHeaderRow
    {
        public int Id { get; init; }
        public int TableId { get; init; }
        public string TableNumber { get; init; } = string.Empty;
        public int? CustomerId { get; init; }
        public string? CustomerName { get; init; }
        public DateTime OrderDateUtc { get; init; }
        public OrderStatus Status { get; init; }
        public decimal Subtotal { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
    }

    private sealed class MenuItemRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}

