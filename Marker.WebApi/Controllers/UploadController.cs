using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marker.WebApi.Controllers;

/// <summary>
/// 文件上传
/// </summary>
[ApiController]
public class UploadController : ControllerBase
{
    private static string Folder => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns>文件地址</returns>
    /// <exception cref="Exception">上传错误</exception>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<string>> UploadAsync(IFormFile file)
    {
        if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
        if (!file.ContentType.Contains("image")) throw new Exception("请上传图片");
        using var crypto = System.Security.Cryptography.SHA256.Create();
        using var input = file.OpenReadStream();
        var hash = await crypto.ComputeHashAsync(input);
        var name = $"{BitConverter.ToString(hash).Replace("-", "")}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var path = Path.Combine(Folder, name);
        var url = $"media/{name}";
        if (System.IO.File.Exists(path)) return new(url);
        using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return new(url);
    }
}
