using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using PolimiProject.Services;

namespace UnitTests.Services;

public class CosmosClientFactoryTests
{
    private readonly Fixture _fixture;

    public CosmosClientFactoryTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void WhenCreateCosmosClient_ShouldRetrieveConnectionStringAndConfigureNamingPolicy()
    {
        var expectedAccountEndpoint = $"https://{_fixture.Create<string>()}:443/";
        var expectedAccountKey = _fixture.Create<string>();
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(expectedAccountKey);
        var expectedAccountKeyBase64 = Convert.ToBase64String(plainTextBytes);
        
        var expectedConnectionString =
            $"AccountEndpoint={expectedAccountEndpoint};AccountKey={expectedAccountKeyBase64};";
        
        var validConfiguration = new Dictionary<string, string>
        {
            {"CosmosDbConnectionString", expectedConnectionString}
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(validConfiguration)
            .Build();
        
        var sut = new CosmosClientFactory(configuration);
        var result = sut.Create();
        
        result.Endpoint.OriginalString.Should().Be(expectedAccountEndpoint);
        result.ClientOptions.SerializerOptions.PropertyNamingPolicy
            .Should().Be(CosmosPropertyNamingPolicy.CamelCase);

    }
}