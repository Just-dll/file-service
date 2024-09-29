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
    .WithReference(cache)
    .WithReference(filedb);

var fileEndpoint = fileService.GetEndpoint("https");

var clientSecret = builder.AddParameter("mesaProjectBff-secret", true);

var bff = builder.AddProject<Projects.FileService_BFF>("mesaproject-bff")
    .WithEnvironment("BFF:ClientSecret", clientSecret);

var webclient = builder.AddNpmApp("WebApp", "../FileService.WebClient")
    .WithEnvironment("BFF__Url", bff.GetEndpoint("https"))
    .WithEnvironment("Identity__Url", identityEndpoint)
    .WithEnvironment("FileService__Url", fileEndpoint)
    .WithHttpsEndpoint(port: 5031, targetPort: 5031, env: "PORT", isProxied: false);

identityService
    .WithEnvironment("Bff__Url", bff.GetEndpoint("https"))
    .WithEnvironment("FileService__Url", fileEndpoint)
    .WithEnvironment("WebClient__Url", webclient.GetEndpoint("https"));


builder.Build().Run();
