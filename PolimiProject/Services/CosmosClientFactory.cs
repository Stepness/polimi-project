using Microsoft.Azure.Cosmos;

namespace PolimiProject.Services;

public class CosmosClientFactory : ICosmosClientFactory
{
    private readonly IConfiguration _configuration;

    public CosmosClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CosmosClient Create()
    {
        var dbConfig = _configuration.GetValue<string>("CosmosDbConnectionString");
        
        var cosmosClient = new CosmosClient(dbConfig,
            new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            }
        );
        
        return cosmosClient;
    } 
}