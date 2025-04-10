using System.Security.Cryptography;
using Marker.WebApi;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
if (builder.Environment.IsDevelopment())
{
    builder.AddServiceDefaults();
    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}
builder.AddRedisClient("redis");
builder.AddMongoDBClient("marker");
services.Configure<WeChatOption>(builder.Configuration.GetSection("WeChat"));
services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 10 * 1024 * 1024);
services.AddSingleton(_ => _.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
services.AddSingleton<IWeChatService, WeChatService>();
services.AddAuthentication().AddAuth();
services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapGet("/login", async (IWeChatService wechat, IDatabase cache, string code) =>
{
    var u = await wechat.GetUserAsync(code);

    var uuid = Guid.NewGuid().ToString().Replace("-", "");
    await cache.StringSetAsync(uuid, u.openid, TimeSpan.FromHours(7));

    return new RS<string>(uuid);
}).WithOpenApi();

app.MapGet("/markers", async (IDatabase cache, IMongoDatabase db, [FromHeader] string token) =>
{
    string openid = (await cache.StringGetAsync(token))!;
    var M = db.GetCollection<CMarker>("Markers");

    var p = Builders<CMarker>.Projection.Include(m => m.Id).Include(m => m.Latitude).Include(m => m.Longitude).Include(m => m.Content).Include(m => m.TagId).Include(m => m.Tag);
    var i = M.Find(m => m.OpenId == openid).Project<MarkerResult>(p).ToListAsync();
    var o = await M.Find(m => m.Share == true && m.OpenId != openid).Project<MarkerResult>(p).ToListAsync();
    return new RS<IEnumerable<MarkerResult>>((await i).Concat(o));
}).RequireAuthorization().WithOpenApi();

app.MapGet("/tegs", async (IMongoDatabase db, string name) =>
{
    var T = db.GetCollection<CTag>("Tags");
    return new RS<IEnumerable<CTag>>(await T.Find(_ => _.Name.Contains(name)).Limit(10).ToListAsync());
}).WithOpenApi();

app.MapGet("/info", async (IDatabase cache, IMongoDatabase db, [FromHeader] string token, int id) =>
{
    string openid = (await cache.StringGetAsync(token))!;
    var M = db.GetCollection<CMarker>("Markers");
    var m = await M.Find(m => m.Id == id).FirstOrDefaultAsync();
    return new RS<InfoResult>(new InfoResult(m.Latitude, m.Longitude, m.Content, m.Tag, m.Images, m.Share, m.OpenId == openid));
}).RequireAuthorization().WithOpenApi();

app.MapPost("/edit", async (IDatabase cache, IMongoDatabase db, [FromHeader] string token, EditForm from) =>
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
}).RequireAuthorization().WithOpenApi();

app.MapPost("/upload", async (HttpRequest request) =>
{
    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
    var file = request?.Form?.Files?.FirstOrDefault() ?? throw new Exception("请上传文件");
    using var input = file.OpenReadStream();
    using var crypto = SHA256.Create();
    var hash = await crypto.ComputeHashAsync(input);

    var name = $"{BitConverter.ToString(hash).Replace("-", "")}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
    var path = Path.Combine(folder, name);
    var url = $"media/{name}";

    if (File.Exists(path)) return new RS<string>(url);
    using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);
    return new RS<string>(url);
}).RequireAuthorization().WithOpenApi();

app.Run();
