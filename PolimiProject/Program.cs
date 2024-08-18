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
        if (cosmosDbConnectionString != null)
        {
            builder.Services.AddScoped<ICosmosLinqQuery, CosmosLinqQuery>();
            builder.Services.AddScoped<ICosmosClientFactory, CosmosClientFactory>();
            builder.Services.AddScoped<IRepositoryData, CosmosRepositoryData>();
            builder.Services.AddScoped<IRepositoryUsers, CosmosRepositoryUsers>();
        }
        else
        {
            builder.Services.AddSingleton<IRepositoryData, InMemoryRepositoryData>();
            builder.Services.AddSingleton<IRepositoryUsers, InMemoryRepositoryUsers>();
        }
        
        const string allowedOrigins = "AllowedOrigins";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: allowedOrigins,
                policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500");
                });
        });
        
        var app = builder.Build();
        
        app.UseCors(allowedOrigins);

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        
        app.Run();
    }
}