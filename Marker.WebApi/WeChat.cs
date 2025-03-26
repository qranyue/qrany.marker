using Microsoft.Extensions.Options;

namespace Marker.WebApi;

public interface IWeChatService
{
    Task<WeChatUser> GetUserAsync(string code);
}

public record WeChatOption
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

#pragma warning disable IDE1006 // 命名样式
public record WeChatUser(string session_key, string openid);
#pragma warning restore IDE1006 // 命名样式

public class WeChatService(IOptions<WeChatOption> option) : IWeChatService
{
    private readonly WeChatOption wechat = option.Value;

    private const string url = "https://api.weixin.qq.com/";

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
