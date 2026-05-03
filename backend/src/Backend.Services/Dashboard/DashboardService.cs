using Backend.Application.Dashboard;
using Backend.Application.Persistence;

namespace Backend.Services.Dashboard;

public sealed class DashboardService(IDashboardRepository repository) : IDashboardService
{
    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        => repository.GetSummaryAsync(cancellationToken);
}
