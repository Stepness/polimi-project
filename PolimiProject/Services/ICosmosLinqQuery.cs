using Microsoft.Azure.Cosmos.Linq;

namespace PolimiProject.Services;

public interface ICosmosLinqQuery
{
    Task<List<T>> ListResultAsync<T>(IQueryable<T> query);
}
