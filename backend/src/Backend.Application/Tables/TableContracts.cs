using Backend.Application.Abstractions;

namespace Backend.Application.Tables;

public sealed record DiningTableDto(int Id, string TableNumber, int Capacity, bool IsOccupied, bool IsActive);
public sealed record DiningTableUpsertRequest(string TableNumber, int Capacity, bool IsOccupied, bool IsActive);

public interface ITableService : ICrudService<DiningTableDto, DiningTableUpsertRequest, DiningTableUpsertRequest>;
