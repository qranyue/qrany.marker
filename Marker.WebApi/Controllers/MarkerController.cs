using Marker.WebApi.Collections;
using Marker.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
        var i = (from _ in M.AsQueryable() where _.OpenId == openid select new MarkerResult(_.Id, _.Latitude, _.Longitude, _.Title, _.Content, _.TagId, _.Tag)).ToListAsync();
        var o = await (from _ in M.AsQueryable() where _.Share == true && _.OpenId != openid select new MarkerResult(_.Id, _.Latitude, _.Longitude, _.Title, _.Content, _.TagId, _.Tag)).ToListAsync();
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
        var m = await (from _ in M.AsQueryable() where _.Id == id select _).FirstOrDefaultAsync();
        return new(new(m.Id, m.Latitude, m.Longitude, m.Title, m.Content, m.Tag, m.Images, m.Share, m.OpenId == openid));
    }

    /// <summary>
    /// 编辑标签
    /// </summary>
    /// <param name="token"></param>
    /// <param name="form">编辑内容</param>
    /// <returns>更新的内容</returns>
    [HttpPost("edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RE), StatusCodes.Status500InternalServerError)]
    public async Task<RS<MarkerResult>> EditAsync([FromHeader] string token, EditForm form)
    {
        string openid = (await cache.StringGetAsync(token))!;
        var M = db.GetCollection<CMarker>("Markers");
        var T = db.GetCollection<CTag>("Tags");
        var t = await (from _ in T.AsQueryable() where _.Name == form.Tag select _).FirstOrDefaultAsync();
        if (null == t)
        {
            var tc = await (from _ in M.AsQueryable() select _).CountAsync();
            t = new CTag(tc, form.Tag);
            await T.InsertOneAsync(t);
        }
        if (form.Id.HasValue)
        {
            await M.UpdateOneAsync(m => m.Id == form.Id, Builders<CMarker>.Update
                .Set(_ => _.Latitude, form.Latitude)
                .Set(_ => _.Longitude, form.Longitude)
                .Set(_ => _.Title, form.Title)
                .Set(_ => _.Content, form.Content)
                .Set(_ => _.TagId, t.Id)
                .Set(_ => _.Tag, form.Tag)
                .Set(_ => _.Images, form.Images)
                .Set(_ => _.Share, form.Share)
            );
            return new(new(form.Id.Value, form.Latitude, form.Longitude, form.Title, form.Content, t.Id, form.Tag));
        }
        else
        {
            var mc = await (from _ in M.AsQueryable() select _).CountAsync();
            var m = new CMarker(mc, openid, form.Latitude, form.Longitude, form.Title, form.Content, t.Id, form.Tag, form.Images, form.Share);
            await M.InsertOneAsync(m);
            return new(new(m.Id, m.Latitude, m.Longitude, m.Title, m.Content, m.TagId, m.Tag));
        }
    }
}