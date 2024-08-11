using Microsoft.Azure.Cosmos;
using PolimiProject.Extensions;
using PolimiProject.Services;

namespace PolimiProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCustomSwaggerConfiguration();
        builder.Services.AddCustomAuthentication();
        builder.Services.AddCustomAuthorization();

        var cosmosDbConnectionString = builder.Configuration["CosmosDbConnectionString"];
        builder.Services.AddScoped<IRepositoryUsers, CosmosRepositoryUsers>();
        builder.Services.AddScoped<IRepositoryData, CosmosRepositoryData>();

        builder.Services.AddSingleton(new CosmosClient(
            cosmosDbConnectionString,
            new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            }
        ));
        
        var app = builder.Build();
        
        app.MapControllers();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.Run();
    }
}