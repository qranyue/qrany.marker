using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marker.WebApi.Controllers;

/// <summary>
/// 文件上传
/// </summary>
[ApiController]
public class UploadController : ControllerBase
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns>文件地址</returns>
    /// <exception cref="Exception">上传错误</exception>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RS<string>>> UploadAsync()
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        var file = Request?.Form?.Files?.FirstOrDefault() ?? throw new Exception("请上传文件");
        using var input = file.OpenReadStream();
        using var crypto = System.Security.Cryptography.SHA256.Create();
        var hash = await crypto.ComputeHashAsync(input);

        var name = $"{BitConverter.ToString(hash).Replace("-", "")}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var path = Path.Combine(folder, name);
        var url = $"media/{name}";

        if (System.IO.File.Exists(path)) return Ok(new RS<string>(url));
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return Ok(new RS<string>(url));
    }
}
