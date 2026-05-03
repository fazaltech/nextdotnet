namespace Backend.Repositories.Initialization;

internal static class DatabaseSchema
{
    public const string Sql = @"
IF OBJECT_ID('dbo.RmsPermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsPermissions
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(128) NOT NULL UNIQUE,
        Description NVARCHAR(256) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsRolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsRolePermissions
    (
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT PK_RmsRolePermissions PRIMARY KEY (RoleId, PermissionId),
        CONSTRAINT FK_RmsRolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RmsRolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.RmsPermissions(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID('dbo.RmsFoodCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsFoodCategories
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsMenuItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsMenuItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Price DECIMAL(18,2) NOT NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_RmsMenuItems_FoodCategories FOREIGN KEY (CategoryId) REFERENCES dbo.RmsFoodCategories(Id)
    );
END

IF OBJECT_ID('dbo.RmsDiningTables', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsDiningTables
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TableNumber NVARCHAR(30) NOT NULL UNIQUE,
        Capacity INT NOT NULL,
        IsOccupied BIT NOT NULL DEFAULT 0,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsCustomers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsCustomers
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(120) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(120) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsSalesOrders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsSalesOrders
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TableId INT NOT NULL,
        CustomerId INT NULL,
        OrderDateUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Status INT NOT NULL,
        Subtotal DECIMAL(18,2) NOT NULL,
        TaxAmount DECIMAL(18,2) NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_RmsSalesOrders_DiningTables FOREIGN KEY (TableId) REFERENCES dbo.RmsDiningTables(Id),
        CONSTRAINT FK_RmsSalesOrders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.RmsCustomers(Id)
    );
END

IF OBJECT_ID('dbo.RmsSalesOrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsSalesOrderItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        MenuItemId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_RmsSalesOrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.RmsSalesOrders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RmsSalesOrderItems_MenuItems FOREIGN KEY (MenuItemId) REFERENCES dbo.RmsMenuItems(Id)
    );
END

IF OBJECT_ID('dbo.RmsInvoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsInvoices
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        InvoiceNumber NVARCHAR(40) NOT NULL UNIQUE,
        InvoiceDateUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status INT NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_RmsInvoices_Orders FOREIGN KEY (OrderId) REFERENCES dbo.RmsSalesOrders(Id)
    );
END

IF OBJECT_ID('dbo.RmsPayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsPayments
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Method INT NOT NULL,
        Status INT NOT NULL,
        PaymentDateUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        TransactionReference NVARCHAR(128) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_RmsPayments_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.RmsInvoices(Id)
    );
END

IF OBJECT_ID('dbo.RmsInventoryItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsInventoryItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(120) NOT NULL,
        Unit NVARCHAR(30) NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        ReorderLevel DECIMAL(18,3) NOT NULL,
        UnitCost DECIMAL(18,2) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsStockTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsStockTransactions
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InventoryItemId INT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        TransactionType INT NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_RmsStockTransactions_InventoryItems FOREIGN KEY (InventoryItemId) REFERENCES dbo.RmsInventoryItems(Id)
    );
END

IF OBJECT_ID('dbo.RmsExpenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsExpenses
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(150) NOT NULL,
        Category NVARCHAR(80) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        ExpenseDateUtc DATETIME2 NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END

IF OBJECT_ID('dbo.RmsAppSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RmsAppSettings
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(120) NOT NULL UNIQUE,
        SettingValue NVARCHAR(MAX) NOT NULL,
        Description NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
";
}


