using Backend.Application.Auth;
using Backend.Application.Customers;
using Backend.Application.Dashboard;
using Backend.Application.Expenses;
using Backend.Application.FoodCategories;
using Backend.Application.Inventory;
using Backend.Application.Invoices;
using Backend.Application.MenuItems;
using Backend.Application.Orders;
using Backend.Application.Payments;
using Backend.Application.Reports;
using Backend.Application.Roles;
using Backend.Application.Settings;
using Backend.Application.Tables;
using Backend.Application.Users;
using Backend.Services.Auth;
using Backend.Services.Customers;
using Backend.Services.Dashboard;
using Backend.Services.Expenses;
using Backend.Services.FoodCategories;
using Backend.Services.Inventory;
using Backend.Services.Invoices;
using Backend.Services.MenuItems;
using Backend.Services.Orders;
using Backend.Services.Payments;
using Backend.Services.Reports;
using Backend.Services.Roles;
using Backend.Services.Settings;
using Backend.Services.Tables;
using Backend.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();

        services.AddScoped<IFoodCategoryService, FoodCategoryService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
