using System.Text;
using PolimiProject.Identity;
using PolimiProject.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PolimiProject.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecureKey)),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/metrics")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return serviceCollection;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(IdentityData.AdminUserPolicy,
                policy => policy.RequireClaim(IdentityData.RoleClaim, Roles.Admin));

            options.AddPolicy(IdentityData.ViewerUserPolicy,
                policy => policy.RequireClaim(IdentityData.RoleClaim, Roles.Admin, Roles.Viewer));
        });

        return serviceCollection;
    }
}