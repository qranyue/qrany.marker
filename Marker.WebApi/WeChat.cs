using Microsoft.Extensions.Options;

namespace Marker.WebApi;

/// <summary>
/// 微信服务
/// </summary>
public interface IWeChatService
{
    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="code">微信授权码</param>
    /// <returns>用户信息</returns>
    Task<WeChatUser> GetUserAsync(string code);
}

/// <summary>
/// 微信配置
/// </summary>
public record WeChatOption
{
    /// <summary>
    /// AppId
    /// </summary>
    public string AppId { get; set; } = string.Empty;
    /// <summary>
    /// AppSecret
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;
}

#pragma warning disable IDE1006 // 命名样式
/// <summary>
/// 微信用户
/// </summary>
/// <param name="session_key"></param>
/// <param name="openid"></param>
public record WeChatUser(string session_key, string openid);
#pragma warning restore IDE1006 // 命名样式

/// <inheritdoc/>
public class WeChatService(IOptions<WeChatOption> option) : IWeChatService
{
    private readonly WeChatOption wechat = option.Value;

    private const string url = "https://api.weixin.qq.com/";

    /// <inheritdoc/>
    public async Task<WeChatUser> GetUserAsync(string code)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"{url}sns/jscode2session?appid={wechat.AppId}&secret={wechat.AppSecret}&js_code={code}&grant_type=authorization_code");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<WeChatUser>();
        if (null == content) throw new Exception("微信登录失败");
        return content;
    }
};
