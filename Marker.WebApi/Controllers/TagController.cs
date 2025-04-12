using Marker.WebApi.Collections;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Marker.WebApi.Controllers;

[ApiController]
public class TagController(IMongoDatabase db) : ControllerBase
{
    [HttpGet("tag")]
    public async Task<RS<IEnumerable<CTag>>> GetAsync(string name)
    {
        var T = db.GetCollection<CTag>("Tags");
        return new(await T.Find(_ => _.Name.Contains(name)).Limit(10).ToListAsync());
    }
}
