var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var password = builder.AddParameter("mesaProjectSqlServer-password", secret: true);

var mssql = builder.AddSqlServer("mesaProjectSqlServer", password, port: 34211)
    .WithDataVolume();

var identitydb = mssql.AddDatabase("identitydb");
var filedb = mssql.AddDatabase("filedb");

var identityService = builder.AddProject<Projects.IdentityService>("mesaproject-identityservice")
    .WithReference(cache)
    .WithReference(identitydb);

var identityEndpoint = identityService.GetEndpoint("https") ?? throw new ArgumentNullException("Failed to retreive endpoint");

var fileService = builder.AddProject<Projects.FileService_WebApi>("mesaproject-fileservice")
    .WithEnvironment("IdentityService__Url", identityEndpoint)
    .WithReference(filedb);

identityService
    .WithEnvironment("FileService__Url", fileService.GetEndpoint("https"));


builder.Build().Run();
