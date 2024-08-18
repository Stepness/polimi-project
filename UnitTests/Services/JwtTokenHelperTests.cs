using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace UnitTests.Services;

public class JwtTokenHelperTests
{
    [Fact]
    public void WhenGenerateJsonWebToken_ShouldIncludeExpectedClaims()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Role = Roles.Admin
        };

        var token = JwtTokenHelper.GenerateJsonWebToken(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Claims.FirstOrDefault(c => c.Type == "Role")?.Value.Should().Be(user.Role);
        jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Username);
        jsonToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
    }
}