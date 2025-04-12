using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Marker.WebApi.Controllers;

[ApiController]
public class LoginController(IWeChatService wechat, IDatabase cache) : ControllerBase
{
    [HttpGet("login")]
    public async Task<RS<string>> LoginAsync(string code)
    {
        var u = await wechat.GetUserAsync(code);

        var uuid = Guid.NewGuid().ToString().Replace("-", "");
        await cache.StringSetAsync(uuid, u.openid, TimeSpan.FromHours(7));

        return new(new(uuid));
    }
}
