using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PolimiProject.Models;
using PolimiProject.Settings;
using Microsoft.IdentityModel.Tokens;

namespace PolimiProject.Services;

public static class JwtTokenHelper
{
    public static string GenerateJsonWebToken(UserEntity model)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecureKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            "Me",
            "You",
            new[]
            {
                new Claim("Role", model.Role),
                new Claim(JwtRegisteredClaimNames.Sub, model.Username)
            },
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}