using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Marker.WebApi.Controllers;

/// <summary>
/// 登录
/// </summary>
/// <param name="wechat">微信</param>
/// <param name="cache">缓存</param>
[ApiController]
public class LoginController(IWeChatService wechat, IDatabase cache) : ControllerBase
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="code">授权编码</param>
    /// <returns>token</returns>
    [HttpGet("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<string>> LoginAsync(string code)
    {
        var u = await wechat.GetUserAsync(code);

        var uuid = Guid.NewGuid().ToString().Replace("-", "");
        await cache.StringSetAsync(uuid, u.openid, TimeSpan.FromHours(7));

        return new(new(uuid));
    }
}
