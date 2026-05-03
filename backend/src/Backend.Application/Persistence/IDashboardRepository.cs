using Backend.Application.Dashboard;

namespace Backend.Application.Persistence;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
