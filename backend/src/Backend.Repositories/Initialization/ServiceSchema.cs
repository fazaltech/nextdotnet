namespace Backend.Repositories.Initialization;

// Additive only. MenuItemRecipes belongs to the legacy MenuItems/InventoryItems FKs.
// Its rows cannot safely be interpreted as references to Rms IDs.
internal static class ServiceSchema
{
    public const string Sql = """
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @schemaLock int;
EXEC @schemaLock=sys.sp_getapplock @Resource='Rms.ServiceSchema',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=30000;
IF @schemaLock<0 THROW 51000,'Could not acquire schema initialization lock.',1;
IF COL_LENGTH('dbo.RmsDiningTables','ServiceStatus') IS NULL
    ALTER TABLE dbo.RmsDiningTables ADD ServiceStatus nvarchar(20) NOT NULL CONSTRAINT DF_RmsTableService DEFAULT 'Available';
IF OBJECT_ID('dbo.RmsDiningSessions','U') IS NULL
CREATE TABLE dbo.RmsDiningSessions(Id int IDENTITY PRIMARY KEY, OpenedAtUtc datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), ClosedAtUtc datetime2 NULL, CreatedBy int NOT NULL REFERENCES dbo.AspNetUsers(Id));
IF OBJECT_ID('dbo.RmsSessionTables','U') IS NULL
CREATE TABLE dbo.RmsSessionTables(SessionId int NOT NULL REFERENCES dbo.RmsDiningSessions(Id), TableId int NOT NULL REFERENCES dbo.RmsDiningTables(Id), PRIMARY KEY(SessionId,TableId));
IF OBJECT_ID('dbo.RmsSeatAssignments','U') IS NULL
CREATE TABLE dbo.RmsSeatAssignments(Id int IDENTITY PRIMARY KEY, SessionId int NOT NULL REFERENCES dbo.RmsDiningSessions(Id), TableId int NOT NULL REFERENCES dbo.RmsDiningTables(Id), CustomerId int NOT NULL REFERENCES dbo.RmsCustomers(Id), Seat int NOT NULL CHECK(Seat>0), UNIQUE(SessionId,TableId,CustomerId,Seat));
IF COL_LENGTH('dbo.RmsSalesOrders','SessionId') IS NULL
    ALTER TABLE dbo.RmsSalesOrders ADD SessionId int NULL REFERENCES dbo.RmsDiningSessions(Id), CreatedBy int NULL REFERENCES dbo.AspNetUsers(Id), ApprovedAtUtc datetime2 NULL;
IF COL_LENGTH('dbo.RmsSalesOrderItems','PreparationStatus') IS NULL
    ALTER TABLE dbo.RmsSalesOrderItems ADD PreparationStatus nvarchar(20) NOT NULL CONSTRAINT DF_RmsItemPreparation DEFAULT 'Received', CustomerId int NULL REFERENCES dbo.RmsCustomers(Id), Seat int NULL, Instructions nvarchar(1000) NULL;
IF OBJECT_ID('dbo.RmsMenuItemRecipes','U') IS NULL
CREATE TABLE dbo.RmsMenuItemRecipes(MenuItemId int NOT NULL REFERENCES dbo.RmsMenuItems(Id), InventoryItemId int NOT NULL REFERENCES dbo.RmsInventoryItems(Id), RequiredQuantity decimal(18,3) NOT NULL CHECK(RequiredQuantity>0), PRIMARY KEY(MenuItemId,InventoryItemId));
IF COL_LENGTH('dbo.RmsStockTransactions','OrderItemId') IS NULL
    ALTER TABLE dbo.RmsStockTransactions ADD OrderItemId int NULL REFERENCES dbo.RmsSalesOrderItems(Id), UnitCost decimal(18,2) NULL, MovementKind nvarchar(30) NULL, CreatedBy int NULL REFERENCES dbo.AspNetUsers(Id);
IF OBJECT_ID('dbo.RmsSuppliers','U') IS NULL
CREATE TABLE dbo.RmsSuppliers(Id int IDENTITY PRIMARY KEY, Name nvarchar(150) NOT NULL, Contact nvarchar(250) NULL);
IF OBJECT_ID('dbo.RmsPurchases','U') IS NULL
CREATE TABLE dbo.RmsPurchases(Id int IDENTITY PRIMARY KEY, SupplierId int NOT NULL REFERENCES dbo.RmsSuppliers(Id), PurchaseDateUtc datetime2 NOT NULL, Reference nvarchar(128) NOT NULL, Discount decimal(18,2) NOT NULL CHECK(Discount>=0), Tax decimal(18,2) NOT NULL CHECK(Tax>=0), PaidAmount decimal(18,2) NOT NULL DEFAULT 0 CHECK(PaidAmount>=0));
IF OBJECT_ID('dbo.RmsPurchaseItems','U') IS NULL
CREATE TABLE dbo.RmsPurchaseItems(Id int IDENTITY PRIMARY KEY, PurchaseId int NOT NULL REFERENCES dbo.RmsPurchases(Id), InventoryItemId int NOT NULL REFERENCES dbo.RmsInventoryItems(Id), Quantity decimal(18,3) NOT NULL CHECK(Quantity>0), ReceivedQuantity decimal(18,3) NOT NULL DEFAULT 0, UnitCost decimal(18,2) NOT NULL CHECK(UnitCost>=0));
IF COL_LENGTH('dbo.RmsInvoices','SessionId') IS NULL
    ALTER TABLE dbo.RmsInvoices ADD SessionId int NULL REFERENCES dbo.RmsDiningSessions(Id), Subtotal decimal(18,2) NULL, Discount decimal(18,2) NOT NULL DEFAULT 0, TaxAmount decimal(18,2) NOT NULL DEFAULT 0, ServiceCharge decimal(18,2) NOT NULL DEFAULT 0;
IF OBJECT_ID('dbo.RmsInvoiceItems','U') IS NULL
CREATE TABLE dbo.RmsInvoiceItems(InvoiceId int NOT NULL REFERENCES dbo.RmsInvoices(Id), OrderItemId int NOT NULL REFERENCES dbo.RmsSalesOrderItems(Id), PRIMARY KEY(InvoiceId,OrderItemId));
IF COL_LENGTH('dbo.RmsInvoices','RefundAmount') IS NULL
    ALTER TABLE dbo.RmsInvoices ADD RefundAmount decimal(18,2) NOT NULL DEFAULT 0;
IF OBJECT_ID('dbo.RmsAuditEvents','U') IS NULL
CREATE TABLE dbo.RmsAuditEvents(Id bigint IDENTITY PRIMARY KEY, Action nvarchar(80) NOT NULL, EntityId int NOT NULL, UserId int NOT NULL REFERENCES dbo.AspNetUsers(Id), AtUtc datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), Reason nvarchar(1000) NULL, Details nvarchar(max) NOT NULL, RequestKey uniqueidentifier NOT NULL UNIQUE);
COMMIT;
""";
}
