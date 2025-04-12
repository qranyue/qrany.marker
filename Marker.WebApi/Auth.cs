using System.Text.Encodings.Web;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Marker.WebApi;

class AuthOptions : AuthenticationSchemeOptions
{
}

class AuthScheme
{
    public const string Scheme = "AuthScheme";
}

class AuthHandler(IOptionsMonitor<AuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("token", out var token)) return AuthenticateResult.NoResult();
        var cache = Context.RequestServices.GetRequiredService<IDatabase>();
        if (cache == null) return AuthenticateResult.Fail("Redis 未启动");
        if (!await cache.KeyExistsAsync($"{token}")) return AuthenticateResult.NoResult();
        await cache.KeyExpireAsync($"{token}", TimeSpan.FromHours(7));
        var ticket = new AuthenticationTicket(new(new System.Security.Claims.ClaimsIdentity(Scheme.Name, "", "")), Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
        await Response.WriteAsJsonAsync(new RE("授权已过期"));
    }
}

/// <summary>
/// 认证扩展
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 添加认证
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddAuth(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthOptions, AuthHandler>(AuthScheme.Scheme, options => { });
    }
}