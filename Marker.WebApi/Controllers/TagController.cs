using Marker.WebApi.Collections;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
        return new(await (from _ in T.AsQueryable() where _.Name.Contains(name) select _).Take(10).ToListAsync());
    }
}
