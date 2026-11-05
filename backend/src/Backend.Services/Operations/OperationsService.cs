using Backend.Application.Operations;

namespace Backend.Services.Operations;

public sealed class OperationsService(IOperationsRepository repository) : IOperationsService
{
    public Task<object> ReadAsync(string section,int userId,bool allOrders,DateTime from,DateTime to,CancellationToken ct)
        =>repository.ReadAsync(section,userId,allOrders,from,to,ct);
    public Task<int> ExecuteAsync(string action,OperationRequest request,int userId,bool manager,CancellationToken ct)
        =>repository.ExecuteAsync(action,request,userId,manager,ct);
    public Task<IReadOnlyList<string>> PermissionsAsync(int userId,CancellationToken ct)=>repository.PermissionsAsync(userId,ct);
}
