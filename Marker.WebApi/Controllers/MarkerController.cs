using Marker.WebApi.Collections;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Marker.WebApi.Controllers;

/// <summary>
/// 标签
/// </summary>
/// <param name="cache">缓存</param>
/// <param name="db">数据</param>
[ApiController]
public class MarkerController(IDatabase cache, IMongoDatabase db) : ControllerBase
{
    /// <summary>
    /// 获取所有标签
    /// </summary>
    /// <param name="token"></param>
    /// <returns>标签列表</returns>
    [HttpGet("markers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<IEnumerable<MarkerResult>>> GetAsync([FromHeader] string token)
    {
        string openid = (await cache.StringGetAsync(token))!;
        var M = db.GetCollection<CMarker>("Markers");

        var p = Builders<CMarker>.Projection.Include(m => m.Id).Include(m => m.Latitude).Include(m => m.Longitude).Include(m => m.Content).Include(m => m.TagId).Include(m => m.Tag);
        var i = M.Find(m => m.OpenId == openid).Project<MarkerResult>(p).ToListAsync();
        var o = await M.Find(m => m.Share == true && m.OpenId != openid).Project<MarkerResult>(p).ToListAsync();
        return new((await i).Concat(o));
    }

    /// <summary>
    /// 获取标签详情
    /// </summary>
    /// <param name="token"></param>
    /// <param name="id">主键</param>
    /// <returns>标签详情</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<InfoResult>> InfoAsync([FromHeader] string token, long id)
    {
        string openid = (await cache.StringGetAsync(token))!;
        var M = db.GetCollection<CMarker>("Markers");
        var m = await M.Find(m => m.Id == id).FirstOrDefaultAsync();
        return new(new(m.Latitude, m.Longitude, m.Content, m.Tag, m.Images, m.Share, m.OpenId == openid));
    }

    /// <summary>
    /// 编辑标签
    /// </summary>
    /// <param name="token"></param>
    /// <param name="from">编辑内容</param>
    /// <returns>更新的内容</returns>
    [HttpPost("edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<MarkerResult>> EditAsync([FromHeader] string token, EditForm from)
    {
        string openid = (await cache.StringGetAsync(token))!;
        var M = db.GetCollection<CMarker>("Markers");
        var T = db.GetCollection<CTag>("Tags");
        var t = await T.Find(t => t.Name == from.Tag).FirstOrDefaultAsync();
        if (null == t)
        {
            var tc = await T.Find(_ => true).CountDocumentsAsync();
            t = new CTag(tc, from.Tag);
            await T.InsertOneAsync(t);
        }
        if (from.Id.HasValue)
        {
            await M.UpdateOneAsync(m => m.Id == from.Id, Builders<CMarker>.Update
                .Set(_ => _.Latitude, from.Latitude)
                .Set(_ => _.Longitude, from.Longitude)
                .Set(_ => _.Content, from.Content)
                .Set(_ => _.TagId, t.Id)
                .Set(_ => _.Tag, from.Tag)
                .Set(_ => _.Images, from.Images)
                .Set(_ => _.Share, from.Share)
            );
            return new RS<MarkerResult>(new(from.Id.Value, from.Latitude, from.Longitude, from.Content, t.Id, from.Tag));
        }
        else
        {
            var mc = await M.Find(_ => true).CountDocumentsAsync();
            var m = new CMarker(mc, openid, from.Latitude, from.Longitude, from.Content, t.Id, from.Tag, from.Images, from.Share);
            await M.InsertOneAsync(m);
            return new RS<MarkerResult>(new(m.Id, m.Latitude, m.Longitude, m.Content, m.TagId, m.Tag));
        }
    }
}