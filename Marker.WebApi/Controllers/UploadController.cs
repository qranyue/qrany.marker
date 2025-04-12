using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marker.WebApi.Controllers;

[ApiController]
public class UploadController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<RS<string>> UploadAsync()
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

        if (System.IO.File.Exists(path)) return new(url);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return new(url);
    }
}
