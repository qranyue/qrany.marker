using Microsoft.Extensions.Options;

namespace Marker.WebApi;

public interface IWeChatService
{
    public Task<WeChatUser> GetUserAsync(string code);
}

public record WeChatOption(string AppId, string AppSecret);


#pragma warning disable IDE1006 // 命名样式
public record WeChatUser(string session_key, string openid);
#pragma warning restore IDE1006 // 命名样式

public class WeChatService(IOptions<WeChatOption> option) : IWeChatService
{
    WeChatOption wechat = option.Value;

    const string url = "https://api.weixin.qq.com/";

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

