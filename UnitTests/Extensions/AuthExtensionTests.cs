using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using PolimiProject.Extensions;
using PolimiProject.Identity;

namespace UnitTests.Extensions;

public class AuthExtensionTests
{
    [Fact]
    public async Task WhenAddCustomAuthentication_JwtSchemaShouldExist()
    {
        var services = new ServiceCollection();
        services.AddCustomAuthentication();
        var provider = services.BuildServiceProvider();

        var authService = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        authService.Should().NotBeNull();
        
        var scheme = await authService.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
        scheme.Should().NotBeNull();
        
    }
    
    [Fact]
    public async Task WhenAddCustomAuthorization_PoliciesShouldExist()
    {
        var services = new ServiceCollection();
        services.AddCustomAuthorization();
        var provider = services.BuildServiceProvider();

        var authService = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var adminUserPolicy = await authService.GetPolicyAsync(IdentityData.AdminUserPolicy);
        var writerUserPolicy = await authService.GetPolicyAsync(IdentityData.WriterUserPolicy);

        adminUserPolicy.Should().NotBeNull();
        writerUserPolicy.Should().NotBeNull();
    }
}