using System.Security.Claims;
using Backend.Application.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Backend.Repositories.Database;
using Backend.Api.Hubs;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Security;

// Consult current role mappings on every request so revocation does not await JWT expiry.
public sealed class RestaurantAccess(IOperationsRepository repository,IDbConnectionFactory factory,IHubContext<RestaurantHub> hub) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller=context.RouteData.Values["controller"]?.ToString();
        if(controller is "Auth" or "Operations") { await next(); return; }
        if(!int.TryParse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),out var userId)) { context.Result=new UnauthorizedResult();return; }
        var permissions=await repository.PermissionsAsync(userId,context.HttpContext.RequestAborted);
        var permission=controller switch
        {
            "FoodCategories"=>"foodcategories.manage", "MenuItems"=>"menuitems.manage",
            "Dashboard"=>"dashboard.view", "Reports"=>"reports.view", "Roles"=>"roles.manage",
            "Users"=>"users.manage", "Tables"=>"tables.manage", "Customers"=>"customers.manage",
            "Inventory"=>"inventory.manage", "Orders"=>"orders.manage", "Invoices"=>"invoices.manage",
            "Payments"=>"payments.manage", "Expenses"=>"expenses.manage", "Settings"=>"settings.manage", _=>""
        };
        bool lookup=HttpMethods.IsGet(context.HttpContext.Request.Method) && controller is "Tables" or "MenuItems" or "FoodCategories" or "Customers";
        if(!permissions.Contains(permission) && !(lookup && permissions.Contains("service.use"))) { context.Result=new ForbidResult();return; }
        foreach(var argument in context.ActionArguments.Values)
        {
            var invalid=argument switch
            {
                Backend.Application.MenuItems.MenuItemUpsertRequest r=>string.IsNullOrWhiteSpace(r.Name)||r.Price<0||decimal.Round(r.Price,2)!=r.Price,
                Backend.Application.Tables.DiningTableUpsertRequest r=>string.IsNullOrWhiteSpace(r.TableNumber)||r.Capacity<1||r.Capacity>1000,
                Backend.Application.Inventory.InventoryItemUpsertRequest r=>string.IsNullOrWhiteSpace(r.Name)||string.IsNullOrWhiteSpace(r.Unit)||r.UnitCost<0||r.ReorderLevel<0,
                Backend.Application.Expenses.ExpenseUpsertRequest r=>string.IsNullOrWhiteSpace(r.Title)||r.Amount<=0||decimal.Round(r.Amount,2)!=r.Amount,
                Backend.Application.Customers.CustomerUpsertRequest r=>string.IsNullOrWhiteSpace(r.FullName),
                Backend.Application.Settings.AppSettingUpsertRequest r=>string.IsNullOrWhiteSpace(r.SettingKey)||string.IsNullOrWhiteSpace(r.SettingValue),
                _=>false
            };
            if(argument is Backend.Application.Settings.AppSettingUpsertRequest setting)
            {
                if(setting.SettingKey is "TaxPercent" or "ServiceChargePercent" or "DiscountPercent")
                    invalid|=!decimal.TryParse(setting.SettingValue,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out var rate)||rate<0||rate>100;
                if(setting.SettingKey=="PaymentMethods")
                    invalid|=setting.SettingValue.Split(',').Any(x=>!int.TryParse(x,out var method)||method<1||method>4);
            }
            if(invalid){context.Result=new BadRequestObjectResult(new{message="Check required names, positive quantities, and nonnegative prices with at most two decimal places."});return;}
        }
        var executed=await next();
        if(!HttpMethods.IsGet(context.HttpContext.Request.Method) && executed.Exception is null && (executed.Result is not IStatusCodeActionResult status || status.StatusCode is null or <400))
        {
            using var db=factory.CreateConnection();
            // Never log request bodies here: user management includes plaintext input passwords.
            await db.ExecuteAsync(new CommandDefinition("INSERT dbo.RmsAuditEvents(Action,EntityId,UserId,Details,RequestKey) VALUES(@action,@id,@userId,@details,@key)",new{action=$"{controller}.{context.RouteData.Values["action"]}",id=int.TryParse(context.RouteData.Values["id"]?.ToString(),out var id)?id:0,userId,details=context.HttpContext.Request.Path.Value??"",key=Guid.NewGuid()},cancellationToken:context.HttpContext.RequestAborted));
            await hub.Clients.Group(RestaurantHub.Group).SendAsync("Changed",new{action=controller},CancellationToken.None);
        }
    }
}
