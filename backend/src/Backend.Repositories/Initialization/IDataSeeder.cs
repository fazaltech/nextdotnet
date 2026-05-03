namespace Backend.Repositories.Initialization;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
