using System.Data;
using System.Text.Json;
using Backend.Application.Operations;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class OperationsRepository(IDbConnectionFactory factory) : IOperationsRepository
{
    public async Task<IReadOnlyList<string>> PermissionsAsync(int userId, CancellationToken ct)
    {
        await using var db = await factory.OpenSqlConnectionAsync(ct);
        return (await db.QueryAsync<string>(new CommandDefinition("""
SELECT DISTINCT p.Name FROM dbo.RmsPermissions p
JOIN dbo.RmsRolePermissions rp ON rp.PermissionId=p.Id
JOIN dbo.AspNetUserRoles ur ON ur.RoleId=rp.RoleId
JOIN dbo.AspNetUsers u ON u.Id=ur.UserId
WHERE ur.UserId=@userId AND u.IsActive=1 AND p.IsActive=1
""", new { userId }, cancellationToken: ct))).ToArray();
    }

    public async Task<object> ReadAsync(string section, int userId, bool allOrders, DateTime from, DateTime to, CancellationToken ct)
    {
        if (to <= from || to > from.AddYears(2)) throw new InvalidOperationException("Choose a date range of up to two years.");
        var sql = section switch
        {
            "payment-methods" => """
SELECT v.Id,v.Name FROM (VALUES(1,'Cash'),(2,'Card'),(3,'Bank transfer'),(4,'Wallet'))v(Id,Name)
WHERE NOT EXISTS(SELECT 1 FROM dbo.RmsAppSettings WHERE SettingKey='PaymentMethods' AND IsActive=1)
OR v.Id IN (SELECT TRY_CONVERT(int,value) FROM dbo.RmsAppSettings s CROSS APPLY STRING_SPLIT(s.SettingValue,',') WHERE s.SettingKey='PaymentMethods' AND s.IsActive=1)
""",
            "tables" => """
SELECT t.*, st.SessionId FROM dbo.RmsDiningTables t
OUTER APPLY(SELECT TOP 1 x.SessionId FROM dbo.RmsSessionTables x JOIN dbo.RmsDiningSessions s ON s.Id=x.SessionId WHERE x.TableId=t.Id AND s.ClosedAtUtc IS NULL) st WHERE t.IsActive=1 ORDER BY t.TableNumber
""",
            "seats" => """
SELECT a.*,c.FullName,t.TableNumber FROM dbo.RmsSeatAssignments a JOIN dbo.RmsCustomers c ON c.Id=a.CustomerId JOIN dbo.RmsDiningTables t ON t.Id=a.TableId JOIN dbo.RmsDiningSessions s ON s.Id=a.SessionId WHERE s.ClosedAtUtc IS NULL
""",
            "orders" => """
SELECT o.*,t.TableNumber,u.FullName Employee FROM dbo.RmsSalesOrders o JOIN dbo.RmsDiningTables t ON t.Id=o.TableId LEFT JOIN dbo.AspNetUsers u ON u.Id=o.CreatedBy WHERE o.IsActive=1 AND (@allOrders=1 OR o.CreatedBy=@userId) ORDER BY o.Id DESC
""",
            "items" => """
SELECT i.*,m.Name,o.SessionId,o.TableId,o.Status OrderStatus,o.CreatedBy FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsMenuItems m ON m.Id=i.MenuItemId JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId WHERE o.IsActive=1 AND (@allOrders=1 OR o.CreatedBy=@userId) ORDER BY i.Id DESC
""",
            "kitchen" => """
SELECT i.*,m.Name,o.SessionId,o.TableId,t.TableNumber FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsMenuItems m ON m.Id=i.MenuItemId JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId JOIN dbo.RmsDiningTables t ON t.Id=o.TableId WHERE o.ApprovedAtUtc IS NOT NULL AND o.Status<>5 AND i.PreparationStatus IN ('Received','Preparing','Ready') ORDER BY o.OrderDateUtc,i.Id
""",
            "recipes" => """
SELECT r.*,m.Name MenuItem,n.Name Ingredient,n.Unit,n.UnitCost,r.RequiredQuantity*n.UnitCost EstimatedCost FROM dbo.RmsMenuItemRecipes r JOIN dbo.RmsMenuItems m ON m.Id=r.MenuItemId JOIN dbo.RmsInventoryItems n ON n.Id=r.InventoryItemId ORDER BY m.Name,n.Name
""",
            "stock" => "SELECT * FROM dbo.RmsInventoryItems WHERE IsActive=1 ORDER BY Name",
            "movements" => "SELECT s.*,i.Name,i.Unit FROM dbo.RmsStockTransactions s JOIN dbo.RmsInventoryItems i ON i.Id=s.InventoryItemId WHERE s.CreatedAtUtc>=@from AND s.CreatedAtUtc<@to ORDER BY s.Id DESC",
            "suppliers" => "SELECT * FROM dbo.RmsSuppliers ORDER BY Name",
            "purchases" => """
SELECT p.*,s.Name Supplier,COALESCE(x.Subtotal,0)-p.Discount+p.Tax Total,COALESCE(x.Subtotal,0)-p.Discount+p.Tax-p.PaidAmount Balance,CASE WHEN p.PaidAmount>=COALESCE(x.Subtotal,0)-p.Discount+p.Tax THEN 'Paid' WHEN p.PaidAmount>0 THEN 'Partially paid' ELSE 'Unpaid' END PaymentStatus FROM dbo.RmsPurchases p JOIN dbo.RmsSuppliers s ON s.Id=p.SupplierId OUTER APPLY(SELECT SUM(Quantity*UnitCost) Subtotal FROM dbo.RmsPurchaseItems WHERE PurchaseId=p.Id)x WHERE p.PurchaseDateUtc>=@from AND p.PurchaseDateUtc<@to ORDER BY p.Id DESC
""",
            "purchase-items" => "SELECT p.*,i.Name,i.Unit FROM dbo.RmsPurchaseItems p JOIN dbo.RmsInventoryItems i ON i.Id=p.InventoryItemId ORDER BY p.Id DESC",
            "bills" => """
SELECT i.*,COALESCE(p.Paid,0) PaidAmount,i.TotalAmount-i.RefundAmount-COALESCE(p.Paid,0) Balance FROM dbo.RmsInvoices i OUTER APPLY(SELECT SUM(Amount) Paid FROM dbo.RmsPayments WHERE InvoiceId=i.Id AND Status=2 AND IsActive=1)p WHERE i.IsActive=1 ORDER BY i.Id DESC
""",
            "receipt-items" => """
SELECT a.InvoiceId,i.*,m.Name FROM dbo.RmsInvoiceItems a JOIN dbo.RmsSalesOrderItems i ON i.Id=a.OrderItemId JOIN dbo.RmsMenuItems m ON m.Id=i.MenuItemId
UNION ALL SELECT b.Id,i.*,m.Name FROM dbo.RmsInvoices b JOIN dbo.RmsSalesOrderItems i ON i.OrderId=b.OrderId JOIN dbo.RmsMenuItems m ON m.Id=i.MenuItemId WHERE b.SessionId IS NULL AND NOT EXISTS(SELECT 1 FROM dbo.RmsInvoiceItems a WHERE a.InvoiceId=b.Id)
""",
            "audit" => "SELECT TOP 500 * FROM dbo.RmsAuditEvents WHERE AtUtc>=@from AND AtUtc<@to ORDER BY Id DESC",
            "sales" => """
SELECT CAST(b.InvoiceDateUtc AS date) SaleDate,SUM((COALESCE(b.Subtotal,b.TotalAmount)-b.Discount)*(1-CASE WHEN b.TotalAmount>0 THEN b.RefundAmount/b.TotalAmount ELSE 0 END)) Sales,SUM(b.TaxAmount) Tax,SUM(b.ServiceCharge) ServiceCharge,SUM(b.Discount) Discounts,SUM(b.TotalAmount-b.RefundAmount) Total FROM dbo.RmsInvoices b WHERE b.Status<>4 AND b.InvoiceDateUtc>=@from AND b.InvoiceDateUtc<@to GROUP BY CAST(b.InvoiceDateUtc AS date) ORDER BY SaleDate
""",
            "sales-items" => """
SELECT m.Name,c.Name Category,t.TableNumber,u.FullName Employee,SUM(i.Quantity) Quantity,SUM(i.LineTotal) Sales FROM dbo.RmsInvoiceItems a JOIN dbo.RmsInvoices b ON b.Id=a.InvoiceId JOIN dbo.RmsSalesOrderItems i ON i.Id=a.OrderItemId JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId JOIN dbo.RmsMenuItems m ON m.Id=i.MenuItemId JOIN dbo.RmsFoodCategories c ON c.Id=m.CategoryId JOIN dbo.RmsDiningTables t ON t.Id=o.TableId LEFT JOIN dbo.AspNetUsers u ON u.Id=o.CreatedBy WHERE b.Status<>4 AND b.InvoiceDateUtc>=@from AND b.InvoiceDateUtc<@to GROUP BY m.Name,c.Name,t.TableNumber,u.FullName
""",
            "payment-report" => "SELECT Method,SUM(Amount) NetPayments FROM dbo.RmsPayments WHERE Status=2 AND PaymentDateUtc>=@from AND PaymentDateUtc<@to GROUP BY Method",
            "recipe-cost" => """
SELECT m.Id,m.Name,m.Price,COALESCE(SUM(r.RequiredQuantity*i.UnitCost),0) EstimatedIngredientCost,m.Price-COALESCE(SUM(r.RequiredQuantity*i.UnitCost),0) EstimatedGrossProfit FROM dbo.RmsMenuItems m LEFT JOIN dbo.RmsMenuItemRecipes r ON r.MenuItemId=m.Id LEFT JOIN dbo.RmsInventoryItems i ON i.Id=r.InventoryItemId GROUP BY m.Id,m.Name,m.Price
""",
            "profit" => """
SELECT s.Sales,c.IngredientCost,s.Sales-c.IngredientCost GrossProfit,e.Expenses,s.Sales-c.IngredientCost-e.Expenses NetProfit FROM
(SELECT COALESCE(SUM((COALESCE(Subtotal,TotalAmount)-Discount)*(1-CASE WHEN TotalAmount>0 THEN RefundAmount/TotalAmount ELSE 0 END)),0) Sales FROM dbo.RmsInvoices WHERE Status<>4 AND InvoiceDateUtc>=@from AND InvoiceDateUtc<@to)s CROSS JOIN
(SELECT COALESCE(SUM(CASE WHEN TransactionType=1 THEN -Quantity ELSE Quantity END*COALESCE(UnitCost,0)),0) IngredientCost FROM dbo.RmsStockTransactions WHERE MovementKind IN ('Consumption','Reversal','Waste') AND CreatedAtUtc>=@from AND CreatedAtUtc<@to)c CROSS JOIN
(SELECT COALESCE(SUM(Amount),0) Expenses FROM dbo.RmsExpenses WHERE IsActive=1 AND ExpenseDateUtc>=@from AND ExpenseDateUtc<@to)e
""",
            _ => throw new InvalidOperationException("Unknown report or section.")
        };
        await using var db = await factory.OpenSqlConnectionAsync(ct);
        return (await db.QueryAsync(new CommandDefinition(sql, new { userId, allOrders, from, to }, cancellationToken: ct))).Select(row => (IDictionary<string, object>)row).ToArray();
    }

    public async Task<int> ExecuteAsync(string action, OperationRequest r, int userId, bool manager, CancellationToken ct)
    {
        if (r.RequestKey == Guid.Empty) throw new InvalidOperationException("A unique request key is required.");
        if (r.Reason?.Length>1000 || r.Reference?.Length>128 || r.Name?.Length>150) throw new InvalidOperationException("Text is too long.");
        if(decimal.Round(r.Amount,2)!=r.Amount || decimal.Round(r.UnitCost,2)!=r.UnitCost || decimal.Round(r.Discount,2)!=r.Discount || decimal.Round(r.Tax,2)!=r.Tax || decimal.Round(r.Quantity,3)!=r.Quantity)
            throw new InvalidOperationException("Money supports two decimal places; stock quantities support three.");
        await using var db = await factory.OpenSqlConnectionAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // One restaurant per database. Serialize financial/service commands, including retries.
        async Task<int> Run(string sql, object? args = null) => await db.ExecuteAsync(new CommandDefinition(sql,args,tx,cancellationToken:ct));
        async Task<T?> Scalar<T>(string sql, object? args = null) => await db.ExecuteScalarAsync<T>(new CommandDefinition(sql,args,tx,cancellationToken:ct));
        async Task<dynamic> Row(string sql, object? args = null) => await db.QuerySingleAsync(new CommandDefinition(sql,args,tx,cancellationToken:ct));
        async Task<IEnumerable<dynamic>> Rows(string sql, object? args = null) => await db.QueryAsync(new CommandDefinition(sql,args,tx,cancellationToken:ct));
        void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
        void Reason() => Require(!string.IsNullOrWhiteSpace(r.Reason),"A reason is required.");
        async Task OpenSession(int id) => Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsDiningSessions WHERE Id=@id AND ClosedAtUtc IS NULL",new{id})==1,"Dining session is closed or missing.");
        async Task MutableOrder(int id,int? itemId=null)
        {
            Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSalesOrders WHERE Id=@id AND IsActive=1 AND Status<>5",new{id})==1,"Order is missing or cancelled.");
            Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsInvoices b WHERE b.Status<>4 AND ((b.SessionId IS NULL AND b.OrderId=@id) OR EXISTS(SELECT 1 FROM dbo.RmsInvoiceItems a JOIN dbo.RmsSalesOrderItems i ON i.Id=a.OrderItemId WHERE a.InvoiceId=b.Id AND i.OrderId=@id AND (@itemId IS NULL OR i.Id=@itemId)))",new{id,itemId})==0,"Billed lines are locked. Use a traceable void/refund first.");
            var sid=await Scalar<int?>("SELECT SessionId FROM dbo.RmsSalesOrders WHERE Id=@id",new{id});
            if(sid.HasValue) await OpenSession(sid.Value);
        }
        async Task MoveStock(int ingredient,decimal delta,string kind,int? item=null,decimal? cost=null)
        {
            Require(delta!=0,"Quantity must not be zero.");
            Require(await Run("UPDATE dbo.RmsInventoryItems SET Quantity=Quantity+@delta, UpdatedAtUtc=SYSUTCDATETIME() WHERE Id=@ingredient AND IsActive=1 AND Quantity+@delta>=0",new{ingredient,delta})==1,$"Insufficient stock or inactive ingredient #{ingredient}; required change: {delta}. Check ingredient stock before approval.");
            await Run("INSERT dbo.RmsStockTransactions(InventoryItemId,Quantity,TransactionType,Notes,OrderItemId,UnitCost,MovementKind,CreatedBy) SELECT Id,ABS(@delta),CASE WHEN @delta>0 THEN 1 ELSE 2 END,@reason,@item,COALESCE(@cost,UnitCost),@kind,@userId FROM dbo.RmsInventoryItems WHERE Id=@ingredient",new{ingredient,delta,reason=r.Reason,item,cost,kind,userId});
        }
        await Run("DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource='Rms.Service',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000; IF @result<0 THROW 51000,'Service is busy. Retry the operation.',1;");
        var previous=await db.QuerySingleOrDefaultAsync(new CommandDefinition("SELECT EntityId,Action,UserId,Details FROM dbo.RmsAuditEvents WHERE RequestKey=@RequestKey",r,tx,cancellationToken:ct));
        var details=JsonSerializer.Serialize(r);
        if(previous is not null)
        {
            Require(previous.Action==action && previous.UserId==userId && previous.Details==details,"Request key has already been used for a different operation.");
            await tx.CommitAsync(ct); return (int)previous.EntityId;
        }
        int result=r.Id;
        switch(action)
        {
            case "adopt-order":
                Reason(); await MutableOrder(r.Id);
                var legacy=await Row("SELECT * FROM dbo.RmsSalesOrders WHERE Id=@Id",r);
                Require(legacy.SessionId==null && legacy.Status==1,"Only unbilled legacy pending orders can be adopted. Historical preparation cannot be inferred.");
                int? existingSession=await Scalar<int?>("SELECT TOP 1 x.SessionId FROM dbo.RmsSessionTables x JOIN dbo.RmsDiningSessions s ON s.Id=x.SessionId WHERE x.TableId=@tableId AND s.ClosedAtUtc IS NULL",new{tableId=(int)legacy.TableId});
                int adoptedSession=existingSession??await Scalar<int>("INSERT dbo.RmsDiningSessions(CreatedBy) VALUES(@userId); SELECT CAST(SCOPE_IDENTITY() AS int)",new{userId});
                if(!existingSession.HasValue) await Run("INSERT dbo.RmsSessionTables VALUES(@adoptedSession,@tableId)",new{adoptedSession,tableId=(int)legacy.TableId});
                await Run("UPDATE dbo.RmsSalesOrders SET SessionId=@adoptedSession,CreatedBy=COALESCE(CreatedBy,@userId) WHERE Id=@Id; UPDATE dbo.RmsDiningTables SET IsOccupied=1,ServiceStatus='Occupied' WHERE Id=@tableId",new{adoptedSession,userId,r.Id,tableId=(int)legacy.TableId}); break;
            case "open-session":
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsDiningTables WHERE Id=@TableId AND IsActive=1 AND IsOccupied=0 AND ServiceStatus='Available'",r)==1,"Table is unavailable.");
                result=await Scalar<int>("INSERT dbo.RmsDiningSessions(CreatedBy) VALUES(@userId); SELECT CAST(SCOPE_IDENTITY() AS int)",new{userId});
                await Run("INSERT dbo.RmsSessionTables VALUES(@result,@TableId); UPDATE dbo.RmsDiningTables SET IsOccupied=1,ServiceStatus='Occupied' WHERE Id=@TableId",new{result,r.TableId});
                break;
            case "table-status":
                Require(r.Status is "Available" or "Reserved" or "Cleaning","Invalid table status.");
                Require(await Run("UPDATE dbo.RmsDiningTables SET ServiceStatus=@Status WHERE Id=@Id AND IsOccupied=0",r)==1,"Occupied tables cannot be released here."); break;
            case "assign-seat":
                await OpenSession(r.SessionId);
                Require(r.CustomerId>0 && r.Seat>0,"Select a customer and positive seat number.");
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSessionTables s JOIN dbo.RmsDiningTables t ON t.Id=s.TableId WHERE s.SessionId=@SessionId AND s.TableId=@TableId AND t.Capacity>=@Seat",r)==1,"Seat is outside the session/table capacity.");
                result=await Scalar<int>("INSERT dbo.RmsSeatAssignments(SessionId,TableId,CustomerId,Seat) VALUES(@SessionId,@TableId,@CustomerId,@Seat); SELECT CAST(SCOPE_IDENTITY() AS int)",r); break;
            case "remove-seat":
                var assignment=await Row("SELECT * FROM dbo.RmsSeatAssignments WHERE Id=@Id",r); await OpenSession((int)assignment.SessionId);
                await Run("DELETE dbo.RmsSeatAssignments WHERE Id=@Id",r); break;
            case "move-table":
            case "combine-session":
                await OpenSession(r.SessionId); Reason();
                if(action=="combine-session")
                {
                    Require(r.TargetId!=r.SessionId,"Choose another session."); await OpenSession(r.TargetId);
                    Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsInvoices WHERE SessionId IN (@SessionId,@TargetId) AND Status<>4",r)==0,"Void bills before combining sessions.");
                    await Run("UPDATE dbo.RmsSessionTables SET SessionId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsSeatAssignments SET SessionId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsSalesOrders SET SessionId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsDiningSessions SET ClosedAtUtc=SYSUTCDATETIME() WHERE Id=@SessionId",r);
                }
                else
                {
                    Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSessionTables WHERE SessionId=@SessionId",r)==1,"Move individual tables before combining sessions.");
                    Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsInvoices WHERE SessionId=@SessionId AND Status<>4",r)==0,"Void bills before moving a session.");
                    Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsDiningTables WHERE Id=@TargetId AND IsActive=1 AND IsOccupied=0 AND ServiceStatus='Available' AND Capacity>=COALESCE((SELECT MAX(Seat) FROM dbo.RmsSeatAssignments WHERE SessionId=@SessionId),0)",r)==1,"Destination table is unavailable or too small.");
                    await Run("UPDATE t SET IsOccupied=0,ServiceStatus='Cleaning' FROM dbo.RmsDiningTables t JOIN dbo.RmsSessionTables s ON s.TableId=t.Id WHERE s.SessionId=@SessionId; UPDATE dbo.RmsSessionTables SET TableId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsSeatAssignments SET TableId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsSalesOrders SET TableId=@TargetId WHERE SessionId=@SessionId; UPDATE dbo.RmsDiningTables SET IsOccupied=1,ServiceStatus='Occupied' WHERE Id=@TargetId",r);
                } break;
            case "close-session":
                await OpenSession(r.SessionId);
                var unsettled=await Scalar<int>("""
SELECT (SELECT COUNT(*) FROM dbo.RmsInvoices WHERE SessionId=@SessionId AND Status IN (1,2))+
(SELECT COUNT(*) FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId WHERE o.SessionId=@SessionId AND o.Status<>5 AND i.PreparationStatus<>'Cancelled' AND NOT EXISTS(SELECT 1 FROM dbo.RmsInvoiceItems a JOIN dbo.RmsInvoices b ON b.Id=a.InvoiceId WHERE a.OrderItemId=i.Id AND b.Status IN (3,5)))
""",r);
                if(unsettled>0){Require(manager,"Session has unsettled bills or unbilled items.");Reason();}
                await Run("UPDATE dbo.RmsDiningSessions SET ClosedAtUtc=SYSUTCDATETIME() WHERE Id=@SessionId; UPDATE t SET IsOccupied=0,ServiceStatus='Cleaning' FROM dbo.RmsDiningTables t JOIN dbo.RmsSessionTables s ON s.TableId=t.Id WHERE s.SessionId=@SessionId",r); result=r.SessionId; break;
            case "submit-order":
                await OpenSession(r.SessionId); Require(r.Items.Length>0 && r.Items.Length<=100,"Add between 1 and 100 items.");
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSessionTables WHERE SessionId=@SessionId AND TableId=@TableId",r)==1,"Table does not belong to the dining session.");
                result=await Scalar<int>("INSERT dbo.RmsSalesOrders(TableId,CustomerId,Status,Subtotal,TaxAmount,TotalAmount,SessionId,CreatedBy) VALUES(@TableId,@CustomerId,1,0,0,0,@SessionId,@userId); SELECT CAST(SCOPE_IDENTITY() AS int)",new{r.TableId,r.CustomerId,r.SessionId,userId});
                foreach(var item in r.Items)
                {
                    Require(item.Quantity>0 && item.Quantity<=1000 && item.Instructions?.Length is not >1000,"Invalid item quantity or instructions.");
                    if(item.Seat.HasValue) Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsDiningTables WHERE Id=@TableId AND @Seat BETWEEN 1 AND Capacity",new{r.TableId,item.Seat})==1,"Invalid seat number.");
                    Require(await Run("INSERT dbo.RmsSalesOrderItems(OrderId,MenuItemId,Quantity,UnitPrice,LineTotal,CustomerId,Seat,Instructions) SELECT @result,Id,@Quantity,Price,Price*@Quantity,@CustomerId,@Seat,@Instructions FROM dbo.RmsMenuItems WHERE Id=@MenuItemId AND IsActive=1 AND IsAvailable=1",new{result,item.MenuItemId,item.Quantity,item.CustomerId,item.Seat,item.Instructions})==1,"Menu item is unavailable.");
                }
                await Run("UPDATE dbo.RmsSalesOrders SET Subtotal=x.Total,TotalAmount=x.Total FROM dbo.RmsSalesOrders o CROSS APPLY(SELECT SUM(LineTotal) Total FROM dbo.RmsSalesOrderItems WHERE OrderId=o.Id)x WHERE o.Id=@result",new{result}); break;
            case "edit-item":
                Reason(); var edit=await Row("SELECT * FROM dbo.RmsSalesOrderItems WHERE Id=@Id",r); await MutableOrder((int)edit.OrderId);
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSalesOrders WHERE Id=@id AND ApprovedAtUtc IS NULL",new{id=(int)edit.OrderId})==1,"Only submitted, unapproved orders can be edited.");
                Require(r.Quantity>0 && r.Quantity<=1000 && decimal.Truncate(r.Quantity)==r.Quantity,"Quantity must be a positive whole number.");
                await Run("UPDATE dbo.RmsSalesOrderItems SET Quantity=@Quantity,LineTotal=UnitPrice*@Quantity,Instructions=@Reason WHERE Id=@Id; UPDATE o SET Subtotal=x.Total,TotalAmount=x.Total FROM dbo.RmsSalesOrders o CROSS APPLY(SELECT SUM(LineTotal) Total FROM dbo.RmsSalesOrderItems WHERE OrderId=o.Id)x WHERE o.Id=@orderId",new{r.Quantity,r.Reason,r.Id,orderId=(int)edit.OrderId}); break;
            case "move-order":
                Reason(); await MutableOrder(r.Id); await OpenSession(r.SessionId);
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSessionTables WHERE SessionId=@SessionId AND TableId=@TableId",r)==1,"Destination table must belong to the selected session.");
                await Run("UPDATE dbo.RmsSalesOrders SET SessionId=@SessionId,TableId=@TableId,UpdatedAtUtc=SYSUTCDATETIME() WHERE Id=@Id; UPDATE dbo.RmsSalesOrderItems SET Seat=NULL WHERE OrderId=@Id",r); break;
            case "approve-order":
                await MutableOrder(r.Id);
                if(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSalesOrders WHERE Id=@Id AND ApprovedAtUtc IS NOT NULL",r)>0) break;
                var needs=await Rows("SELECT i.Id,r.InventoryItemId,i.Quantity*r.RequiredQuantity Needed FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsMenuItemRecipes r ON r.MenuItemId=i.MenuItemId WHERE i.OrderId=@Id AND i.PreparationStatus<>'Cancelled' ORDER BY r.InventoryItemId,i.Id",r);
                foreach(var need in needs) await MoveStock((int)need.InventoryItemId,-(decimal)need.Needed,"Consumption",(int)need.Id);
                await Run("UPDATE dbo.RmsSalesOrders SET Status=2,ApprovedAtUtc=SYSUTCDATETIME(),UpdatedAtUtc=SYSUTCDATETIME() WHERE Id=@Id",r); break;
            case "item-status":
            case "cancel-item":
                var line=await Row("SELECT i.*,o.ApprovedAtUtc,o.CreatedBy FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId WHERE i.Id=@Id",r);
                await MutableOrder((int)line.OrderId,r.Id);
                if(action=="cancel-item")
                {
                    Reason(); if(line.PreparationStatus=="Cancelled") break;
                    if(line.ApprovedAtUtc!=null)
                    {
                        if(line.PreparationStatus=="Received")
                            foreach(var consumed in await Rows("SELECT * FROM dbo.RmsStockTransactions WHERE OrderItemId=@Id AND MovementKind='Consumption'",r))
                                await MoveStock((int)consumed.InventoryItemId,(decimal)consumed.Quantity,"Reversal",r.Id,(decimal?)consumed.UnitCost);
                        else await Run("UPDATE dbo.RmsStockTransactions SET MovementKind='Waste',Notes=@Reason WHERE OrderItemId=@Id AND MovementKind='Consumption'",r);
                    }
                    await Run("UPDATE dbo.RmsSalesOrderItems SET PreparationStatus='Cancelled' WHERE Id=@Id",r);
                }
                else
                {
                    Require(line.ApprovedAtUtc!=null,"Manager approval is required before preparation.");
                    string current=line.PreparationStatus;
                    Require((current,r.Status) is ("Received","Preparing") or ("Preparing","Ready") or ("Ready","Served"),"Invalid item status transition.");
                    if(r.Status=="Served") Require(manager || line.CreatedBy==userId,"Only the submitting employee or manager may serve this order.");
                    await Run("UPDATE dbo.RmsSalesOrderItems SET PreparationStatus=@Status WHERE Id=@Id",r);
                }
                await Run("""
UPDATE o SET Subtotal=x.Total,TotalAmount=x.Total+o.TaxAmount,Status=CASE WHEN x.Active=0 THEN 5 WHEN x.Served=x.Active THEN 4 WHEN x.Ready+x.Served=x.Active THEN 3 ELSE o.Status END,UpdatedAtUtc=SYSUTCDATETIME()
FROM dbo.RmsSalesOrders o CROSS APPLY(SELECT COALESCE(SUM(CASE WHEN PreparationStatus<>'Cancelled' THEN LineTotal ELSE 0 END),0) Total,SUM(CASE WHEN PreparationStatus<>'Cancelled' THEN 1 ELSE 0 END) Active,SUM(CASE WHEN PreparationStatus='Served' THEN 1 ELSE 0 END) Served,SUM(CASE WHEN PreparationStatus='Ready' THEN 1 ELSE 0 END) Ready FROM dbo.RmsSalesOrderItems WHERE OrderId=o.Id)x WHERE o.Id=@id
""",new{id=(int)line.OrderId}); break;
            case "reject-order":
                Reason(); await MutableOrder(r.Id);
                Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsSalesOrders WHERE Id=@Id AND ApprovedAtUtc IS NULL",r)==1,"Cancel approved items individually to account for stock.");
                await Run("UPDATE dbo.RmsSalesOrders SET Status=5,Subtotal=0,TotalAmount=0 WHERE Id=@Id; UPDATE dbo.RmsSalesOrderItems SET PreparationStatus='Cancelled' WHERE OrderId=@Id",r); break;
            case "recipe":
                Require(r.Quantity>0,"Recipe quantity must be positive in the ingredient's stock unit.");
                await Run("UPDATE dbo.RmsMenuItemRecipes SET RequiredQuantity=@Quantity WHERE MenuItemId=@MenuItemId AND InventoryItemId=@InventoryItemId; IF @@ROWCOUNT=0 INSERT dbo.RmsMenuItemRecipes VALUES(@MenuItemId,@InventoryItemId,@Quantity)",r); result=r.MenuItemId; break;
            case "remove-recipe":
                Reason(); await Run("DELETE dbo.RmsMenuItemRecipes WHERE MenuItemId=@MenuItemId AND InventoryItemId=@InventoryItemId",r); result=r.MenuItemId; break;
            case "stock-adjust":
            case "stock-count":
            case "waste":
                Reason(); decimal delta=r.Quantity;
                if(action=="stock-count") { Require(r.Quantity>=0,"Count cannot be negative.");delta-=await Scalar<decimal>("SELECT Quantity FROM dbo.RmsInventoryItems WHERE Id=@InventoryItemId",r); }
                if(action=="waste"){Require(r.Quantity>0,"Waste quantity must be positive.");delta=-r.Quantity;}
                if(delta!=0) await MoveStock(r.InventoryItemId,delta,action=="waste"?"Waste":"Adjustment"); result=r.InventoryItemId; break;
            case "supplier":
                Require(!string.IsNullOrWhiteSpace(r.Name),"Supplier name is required."); result=await Scalar<int>("INSERT dbo.RmsSuppliers(Name,Contact) VALUES(@Name,@Reference); SELECT CAST(SCOPE_IDENTITY() AS int)",r); break;
            case "purchase":
                Require(!string.IsNullOrWhiteSpace(r.Reference) && r.Discount>=0 && r.Tax>=0,"Reference, nonnegative discount and tax are required.");
                result=await Scalar<int>("INSERT dbo.RmsPurchases(SupplierId,PurchaseDateUtc,Reference,Discount,Tax) VALUES(@TargetId,COALESCE(@Date,SYSUTCDATETIME()),@Reference,@Discount,@Tax); SELECT CAST(SCOPE_IDENTITY() AS int)",r); break;
            case "purchase-item":
                Require(r.Quantity>0 && r.UnitCost>=0,"Positive quantity and nonnegative unit cost required.");
                Require(await Scalar<decimal>("SELECT PaidAmount FROM dbo.RmsPurchases WHERE Id=@Id",r)==0,"Paid purchases are locked. Record a separate correction purchase.");
                result=await Scalar<int>("INSERT dbo.RmsPurchaseItems(PurchaseId,InventoryItemId,Quantity,UnitCost) VALUES(@Id,@InventoryItemId,@Quantity,@UnitCost); SELECT CAST(SCOPE_IDENTITY() AS int)",r); break;
            case "receive":
                Require(r.Quantity>0,"Received quantity must be positive.");
                var purchaseLine=await Row("SELECT * FROM dbo.RmsPurchaseItems WHERE Id=@Id",r);
                Require(purchaseLine.ReceivedQuantity+r.Quantity<=purchaseLine.Quantity,"Receipt exceeds remaining ordered quantity.");
                await MoveStock((int)purchaseLine.InventoryItemId,r.Quantity,"Receipt",null,(decimal)purchaseLine.UnitCost);
                await Run("UPDATE dbo.RmsPurchaseItems SET ReceivedQuantity=ReceivedQuantity+@Quantity WHERE Id=@Id; UPDATE dbo.RmsInventoryItems SET UnitCost=@cost WHERE Id=@ingredient",new{r.Id,r.Quantity,cost=(decimal)purchaseLine.UnitCost,ingredient=(int)purchaseLine.InventoryItemId}); break;
            case "correct-purchase-item":
                Reason(); Require(r.Quantity>0 && r.UnitCost>=0,"Positive quantity and nonnegative unit cost required.");
                Require(await Run("UPDATE l SET Quantity=@Quantity,UnitCost=@UnitCost FROM dbo.RmsPurchaseItems l JOIN dbo.RmsPurchases p ON p.Id=l.PurchaseId WHERE l.Id=@Id AND l.ReceivedQuantity=0 AND p.PaidAmount=0",r)==1,"Only unreceived, unpaid purchase lines can be corrected. Use a stock adjustment for received goods."); break;
            case "purchase-payment":
                Require(r.Amount>0,"Payment must be positive.");
                Require(await Run("UPDATE p SET PaidAmount=PaidAmount+@Amount FROM dbo.RmsPurchases p WHERE p.Id=@Id AND PaidAmount+@Amount<=(SELECT SUM(Quantity*UnitCost) FROM dbo.RmsPurchaseItems WHERE PurchaseId=p.Id)-p.Discount+p.Tax",r)==1,"Payment exceeds the purchase balance."); break;
            case "bill":
                await OpenSession(r.SessionId);
                var ids=r.ItemIds.Distinct().ToArray(); Require(ids.Length>0,"Select items to bill.");
                var billed=await Rows("""
SELECT i.*,o.SessionId,o.ApprovedAtUtc FROM dbo.RmsSalesOrderItems i JOIN dbo.RmsSalesOrders o ON o.Id=i.OrderId WHERE i.Id IN @ids AND o.SessionId=@SessionId AND o.Status<>5 AND i.PreparationStatus='Served' AND NOT EXISTS(SELECT 1 FROM dbo.RmsInvoiceItems a JOIN dbo.RmsInvoices b ON b.Id=a.InvoiceId WHERE a.OrderItemId=i.Id AND b.Status<>4)
""",new{ids,r.SessionId});
                var billLines=billed.ToArray(); Require(billLines.Length==ids.Length,"Only served, unbilled items in this session can be billed.");
                decimal subtotal=billLines.Sum(x=>(decimal)x.LineTotal);
                var rates=(await Rows("SELECT SettingKey,TRY_CONVERT(decimal(9,4),SettingValue) Rate FROM dbo.RmsAppSettings WHERE IsActive=1 AND SettingKey IN ('TaxPercent','ServiceChargePercent','DiscountPercent')")).ToDictionary(x=>(string)x.SettingKey,x=>x.Rate==null?0m:(decimal)x.Rate);
                decimal Rate(string key){var rate=rates.GetValueOrDefault(key);Require(rate>=0 && rate<=100,"Configured billing percentages must be between 0 and 100.");return rate;}
                decimal discount=decimal.Round(subtotal*Rate("DiscountPercent")/100,2,MidpointRounding.AwayFromZero);
                decimal tax=decimal.Round((subtotal-discount)*Rate("TaxPercent")/100,2,MidpointRounding.AwayFromZero);
                decimal service=decimal.Round((subtotal-discount)*Rate("ServiceChargePercent")/100,2,MidpointRounding.AwayFromZero);
                decimal total=subtotal-discount+tax+service;
                result=await Scalar<int>("INSERT dbo.RmsInvoices(OrderId,InvoiceNumber,TotalAmount,Status,SessionId,Subtotal,Discount,TaxAmount,ServiceCharge) VALUES(@orderId,@number,@total,@status,@SessionId,@subtotal,@discount,@tax,@service); SELECT CAST(SCOPE_IDENTITY() AS int)",new{orderId=(int)billLines[0].OrderId,number=$"INV-{Guid.NewGuid():N}",total,status=total==0?3:1,r.SessionId,subtotal,discount,tax,service});
                foreach(int item in ids) await Run("INSERT dbo.RmsInvoiceItems VALUES(@result,@item)",new{result,item}); break;
            case "pay":
            case "refund":
                Require(r.Amount>0 && r.Method is >=1 and <=4,"Positive amount and valid payment method required.");
                if(action=="pay") Require(await Scalar<int>("SELECT COUNT(*) FROM dbo.RmsAppSettings WHERE SettingKey='PaymentMethods' AND IsActive=1 AND NOT EXISTS (SELECT 1 FROM STRING_SPLIT(SettingValue,',') WHERE TRY_CONVERT(int,value)=@Method)",r)==0,"This payment method is disabled in restaurant settings.");
                var bill=await Row("SELECT * FROM dbo.RmsInvoices WHERE Id=@Id AND IsActive=1",r);
                Require(bill.Status!=4,"Voided bills cannot receive payments.");
                decimal paid=await Scalar<decimal>("SELECT COALESCE(SUM(Amount),0) FROM dbo.RmsPayments WHERE InvoiceId=@Id AND Status=2 AND IsActive=1",r);
                if(action=="refund") {Reason(); Require(r.Amount<=paid,"Refund exceeds payments received.");}
                else Require(r.Amount<=bill.TotalAmount-bill.RefundAmount-paid,"Payment exceeds outstanding balance.");
                decimal amount=action=="refund"?-r.Amount:r.Amount;
                result=await Scalar<int>("INSERT dbo.RmsPayments(InvoiceId,Amount,Method,Status,TransactionReference) VALUES(@Id,@amount,@Method,2,@Reference); SELECT CAST(SCOPE_IDENTITY() AS int)",new{r.Id,amount,r.Method,r.Reference});
                await Run("UPDATE dbo.RmsInvoices SET RefundAmount=RefundAmount+@refund,Status=CASE WHEN RefundAmount+@refund>=TotalAmount AND TotalAmount>0 THEN 5 WHEN @paid>=TotalAmount-RefundAmount-@refund THEN 3 WHEN @paid>0 THEN 2 ELSE 1 END,UpdatedAtUtc=SYSUTCDATETIME() WHERE Id=@Id",new{r.Id,paid=paid+amount,refund=action=="refund"?r.Amount:0}); break;
            case "void-bill":
                Reason(); Require(await Scalar<decimal>("SELECT COALESCE(SUM(Amount),0) FROM dbo.RmsPayments WHERE InvoiceId=@Id AND Status=2",r)==0,"Refund all payments before voiding this bill.");
                Require(await Run("UPDATE dbo.RmsInvoices SET Status=4,UpdatedAtUtc=SYSUTCDATETIME() WHERE Id=@Id AND IsActive=1",r)==1,"Bill not found."); break;
            default: throw new InvalidOperationException("Unknown operation.");
        }
        await Run("INSERT dbo.RmsAuditEvents(Action,EntityId,UserId,Reason,Details,RequestKey) VALUES(@action,@result,@userId,@Reason,@details,@RequestKey)",new{action,result,userId,r.Reason,details,r.RequestKey});
        await tx.CommitAsync(ct);
        return result;
    }
}
