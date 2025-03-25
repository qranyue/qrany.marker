var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var mongo = builder.AddMongoDB("mongodb").AddDatabase("marker");

builder.AddProject<Projects.Marker_WebApi>("webapi").WithReference(redis).WithReference(mongo);

builder.Build().Run();
