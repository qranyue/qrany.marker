using Marker.WebApi;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRedisClient("redis");
builder.AddMongoDBClient("marker");
builder.Services.Configure<WeChatOption>(builder.Configuration.GetSection("WeChat"));
builder.Services.AddSingleton(_ => _.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
builder.Services.AddSingleton<IWeChatService, WeChatService>();
builder.Services.AddAuthentication().AddAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/login", async ([FromServices] IWeChatService wechat, [FromServices] IDatabase cache, string code) =>
{
    var u = await wechat.GetUserAsync(code);

    var uuid = Guid.NewGuid().ToString().Replace("-", "");
    await cache.StringSetAsync(uuid, u.openid, TimeSpan.FromHours(7));

    return new RS<string>(uuid);
}).WithName("登录").WithOpenApi();

app.MapGet("/markers", async ([FromServices] IDatabase cache, [FromServices] IMongoDatabase db, [FromHeader] string token) =>
{
    string openid = (await cache.StringGetAsync(token))!;
    var M = db.GetCollection<CMarker>("Markers");

    var p = Builders<CMarker>.Projection.Include(m => m.Id).Include(m => m.Latitude).Include(m => m.Longitude).Include(m => m.Title).Include(m => m.TagId).Include(m => m.Tag);
    var i = M.Find(m => m.OpenId == openid).Project<MarkerResult>(p).ToListAsync();
    var o = await M.Find(m => m.Share == true && m.OpenId != openid).Project<MarkerResult>(p).ToListAsync();
    return new RS<IEnumerable<MarkerResult>>((await i).Concat(o));
}).RequireAuthorization().WithName("获取").WithOpenApi();

app.MapGet("/tegs", async ([FromServices] IMongoDatabase db, string name) =>
{
    var T = db.GetCollection<CTag>("Tags");
    return new RS<IEnumerable<CTag>>(await T.Find(_ => _.Name.Contains(name)).Limit(10).ToListAsync());
}).WithName("标签").WithOpenApi();

app.MapGet("/info", async ([FromServices] IDatabase cache, [FromServices] IMongoDatabase db, [FromHeader] string token, int id) =>
{
    string openid = (await cache.StringGetAsync(token))!;
    var M = db.GetCollection<CMarker>("Markers");
    var m = await M.Find(m => m.Id == id).FirstOrDefaultAsync();
    return new RS<InfoResult>(new InfoResult(m.Latitude, m.Longitude, m.Title, m.Tag, m.Images, m.Share, m.OpenId == openid));
}).RequireAuthorization().WithName("详情").WithOpenApi();

app.MapPost("/edit", async ([FromServices] IDatabase cache, [FromServices] IMongoDatabase db, [FromHeader] string token, EditForm from) =>
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
            .Set(_ => _.Title, from.Title)
            .Set(_ => _.TagId, t.Id)
            .Set(_ => _.Tag, from.Tag)
            .Set(_ => _.Images, from.Images)
            .Set(_ => _.Share, from.Share)
        );
        return new RS<MarkerResult>(new(from.Id.Value, from.Latitude, from.Longitude, from.Title, t.Id, from.Tag));
    }
    else
    {
        var mc = await M.Find(_ => true).CountDocumentsAsync();
        var m = new CMarker(mc, openid, from.Latitude, from.Longitude, from.Title, t.Id, from.Tag, from.Images, from.Share);
        await M.InsertOneAsync(m);
        return new RS<MarkerResult>(new(m.Id, m.Latitude, m.Longitude, m.Title, m.TagId, m.Tag));
    }
}).RequireAuthorization().WithName("编辑").WithOpenApi();

app.Run();
