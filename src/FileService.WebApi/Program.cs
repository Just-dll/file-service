using AutoMapper;
using FileService.BLL.Grpc;
using FileService.BLL.Interfaces;
using FileService.BLL.MapperProfiles;
using FileService.BLL.Models;
using FileService.BLL.Services;
using FileService.DAL.Data;
using FileService.DAL.Interfaces;
using FileService.DAL.Repositories;
using FileService.WebApi.Filters;
using FileService.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Principal;

namespace FileService.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddDbContext<FileServiceDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("filedb"), builder =>
            {
                builder.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                builder.MigrationsAssembly(typeof(FileServiceDbContext).Assembly.FullName);
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost4200", builder =>
            {
                builder.WithOrigins("https://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });

        builder.Services.AddControllers()
            .AddJsonOptions(opt => opt.JsonSerializerOptions.IncludeFields = true);
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IFileService, BLL.Services.FileService>();
        builder.Services.AddScoped<IFolderService, FolderService>();
        builder.Services.AddScoped<IAccessService, AccessService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IStorageProvider>(sp =>
            new StorageProvider(Path.Combine(Directory.GetCurrentDirectory(), "FileServiceStorage")));
        builder.Services.AddScoped<FolderAccessFilter>();
        builder.Services.AddAutoMapper(config => config.AddProfile<AutoMapperProfile>());

        var identitySection = configuration.GetSection("IdentityService") ?? throw new ArgumentNullException(nameof(args));

        builder.Services.AddAuthentication("token")
            .AddJwtBearer("token", options =>
            {
                options.Authority = identitySection["Url"];
                options.Audience = "https://localhost:5001/resources";
                options.MapInboundClaims = false;

                options.SaveToken = true;

                // options.ForwardChallenge = "oidc";
            });

        builder.Services.AddGrpcClient<Identity.IdentityClient>(options =>
        {
            options.Address = new Uri(identitySection["Url"]);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return handler;
        });

        //builder.Services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = builder.Configuration.GetConnectionString("cache");
        //});

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();

        app.UseCors("AllowLocalhost4200");

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseMiddleware<IdentityCheckerMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        // Mapping discovery endpoint
        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger/v1/swagger.json");
            return Task.CompletedTask;
        });

        app.Run();
    }
}
