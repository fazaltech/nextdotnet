using System.Security.Claims;
using Backend.Application.Operations;
using Backend.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Controllers;

[ApiController, Authorize, Route("api/service")]
public sealed class OperationsController(IOperationsService repository,IHubContext<RestaurantHub> hub) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("permissions")]
    public Task<IReadOnlyList<string>> Permissions(CancellationToken ct)=>repository.PermissionsAsync(UserId,ct);

    [HttpGet("{section}")]
    public async Task<IActionResult> Read(string section,[FromQuery] DateTime? from,[FromQuery] DateTime? to,CancellationToken ct)
    {
        var permissions=await repository.PermissionsAsync(UserId,ct);
        var required=section switch
        {
            "tables" or "seats" or "orders" or "items"=>"service.use",
            "kitchen"=>"kitchen.use", "recipes" or "stock" or "movements"=>"inventory.manage",
            "suppliers" or "purchases" or "purchase-items"=>"purchases.manage",
            "bills" or "receipt-items"=>permissions.Contains("invoices.manage")?"invoices.manage":"payments.manage",
            "payment-methods"=>permissions.Contains("payments.manage")?"payments.manage":"service.use", _=>"reports.view"
        };
        if(!permissions.Contains(required)) return Forbid();
        return Ok(await repository.ReadAsync(section,UserId,permissions.Contains("orders.manage"),from??DateTime.UtcNow.Date.AddDays(-30),to??DateTime.UtcNow.Date.AddDays(1),ct));
    }

    [HttpPost("{operation}")]
    public async Task<IActionResult> Execute(string operation,[FromBody] OperationRequest request,CancellationToken ct)
    {
        var action=operation;
        var permissions=await repository.PermissionsAsync(UserId,ct);
        var required=action switch
        {
            "open-session" or "assign-seat" or "remove-seat" or "submit-order" or "close-session"=>"service.use",
            "move-table" or "combine-session" or "table-status"=>"tables.manage",
            "approve-order" or "reject-order" or "cancel-item" or "edit-item" or "move-order" or "adopt-order"=>"orders.manage",
            "item-status"=>request.Status=="Served"?"service.use":"kitchen.use",
            "recipe" or "remove-recipe" or "stock-adjust" or "stock-count" or "waste"=>"inventory.manage",
            "supplier" or "purchase" or "purchase-item" or "receive" or "purchase-payment" or "correct-purchase-item"=>"purchases.manage",
            "bill"=>"invoices.manage", "pay"=>"payments.manage", "refund" or "void-bill"=>"refunds.manage", _=>""
        };
        if(!permissions.Contains(required)) return Forbid();
        int id=await repository.ExecuteAsync(action,request,UserId,permissions.Contains("orders.manage"),ct);
        // Notifications contain no operational/customer data; each client reloads authorized state.
        await hub.Clients.Group(RestaurantHub.Group).SendAsync("Changed",new{action},CancellationToken.None);
        return Ok(new{id});
    }
}
