using Microsoft.Azure.Cosmos;

namespace PolimiProject.Services;

public class RepositoryDataFactory
{
    public IRepositoryData CreateRepository(string dbConnectionString)
    {
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            return new InMemoryRepositoryData();
        }
        else
        {
            var cosmos = new CosmosClient(
                dbConnectionString,
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                }
            );
            
            return new CosmosRepositoryData(cosmos);
        }
    }
}