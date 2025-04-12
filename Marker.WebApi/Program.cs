using Marker.WebApi;
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
services.AddSingleton(_ => _.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
services.AddSingleton<IWeChatService, WeChatService>();
services.AddAuthentication().AddAuth();
services.AddAuthorization();
services.AddControllers(_ => _.Filters.Add<ExceptionFilter>()); ;

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

app.MapControllers();

app.Run();
