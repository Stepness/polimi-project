using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace PolimiProject.Services;

public interface ICosmosLinqQuery
{
    Task<List<T>> ListResultAsync<T>(IQueryable<T> query);
}

public class CosmosLinqQuery : ICosmosLinqQuery
{
    public async Task<List<T>> ListResultAsync<T>(IQueryable<T> query)
    {
        var iterator = query.ToFeedIterator();
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        
        return results;
    }
}