using System.Security.Claims;
using Backend.Application.Operations;
using Backend.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api;

// Compatibility adapter: old write routes use the same transactional implementation.
public sealed class ServiceBridge(IOperationsRepository repository,IHttpContextAccessor accessor,IHubContext<RestaurantHub> hub)
{
    public async Task<int> Run(string action,OperationRequest request,CancellationToken ct)
    {
        var context=accessor.HttpContext!;
        int userId=int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions=await repository.PermissionsAsync(userId,ct);
        var key=Guid.TryParse(context.Request.Headers["Idempotency-Key"],out var supplied)?supplied:Guid.NewGuid();
        int id=await repository.ExecuteAsync(action,request with {RequestKey=key},userId,permissions.Contains("orders.manage"),ct);
        await hub.Clients.Group(RestaurantHub.Group).SendAsync("Changed",new{action},CancellationToken.None);
        return id;
    }
    public async Task<int> SessionForTable(int tableId,CancellationToken ct)
    {
        var rows=(IDictionary<string,object>[])await repository.ReadAsync("tables",0,true,DateTime.UtcNow.Date,DateTime.UtcNow.Date.AddDays(1),ct);
        var row=rows.FirstOrDefault(x=>(int)x["Id"]==tableId) ?? throw new InvalidOperationException("Table not found.");
        if(row["SessionId"] is int id) return id;
        throw new InvalidOperationException("Open a dining session for this table first at /api/service/open-session.");
    }
}
