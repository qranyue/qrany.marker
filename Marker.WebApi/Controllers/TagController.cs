using Marker.WebApi.Collections;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Marker.WebApi.Controllers;

/// <summary>
/// 标签
/// </summary>
/// <param name="db">数据</param>
[ApiController]
public class TagController(IMongoDatabase db) : ControllerBase
{
    /// <summary>
    /// 获取标签
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [HttpGet("tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<IEnumerable<CTag>>> SearchAsync(string name)
    {
        var T = db.GetCollection<CTag>("Tags");
        return new(await T.Find(_ => _.Name.Contains(name)).Limit(10).ToListAsync());
    }
}
