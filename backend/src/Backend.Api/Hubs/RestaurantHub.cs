using System.Security.Claims;
using Backend.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Api.Hubs;

[Authorize]
public sealed class RestaurantHub(IOperationsRepository repository) : Hub
{
    // The existing schema is single restaurant/database; no branch identity exists.
    public const string Group="restaurant";
    public override async Task OnConnectedAsync()
    {
        if(!int.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier),out var id) ||
           (await repository.PermissionsAsync(id,Context.ConnectionAborted)).Count==0)
        { Context.Abort(); return; }
        await Groups.AddToGroupAsync(Context.ConnectionId,Group);
        await base.OnConnectedAsync();
    }
}
