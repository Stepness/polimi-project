using Microsoft.Azure.Cosmos;

namespace PolimiProject.Services;

public interface ICosmosClientFactory
{
    CosmosClient Create();
}