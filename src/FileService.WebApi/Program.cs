
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
using MesaProject.ResearchService.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.EntityFrameworkCore;
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
        builder.Services.AddControllers()
            .AddJsonOptions(opt => opt.JsonSerializerOptions.IncludeFields = true);
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IFileService, FileService.BLL.Services.FileService>();
        builder.Services.AddScoped<IFolderService, FolderService>();
        builder.Services.AddScoped<IAccessService, AccessService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IStorageProvider>(sp =>
            new StorageProvider(Path.Combine(Directory.GetCurrentDirectory(), "FileServiceStorage")));
        builder.Services.AddScoped<FolderAccessFilter>();
        builder.Services.AddAutoMapper(config => config.AddProfile<AutoMapperProfile>());

        var identitySection = configuration.GetSection("IdentityService") ?? throw new ArgumentNullException(nameof(args));
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options => options.Cookie.Name = ".AspNetCore.FileServiceCookie")
        .AddOpenIdConnect(options =>
        {
            options.Authority = identitySection["Url"];
            options.CallbackPath = "/signin-oidc";
            options.ClientId = "fileService";
            options.ClientSecret = "3B7E82F9-C45B-11EC-952C-0242AC120002";
            options.ResponseType = "code";
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.SaveTokens = true;
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

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseMiddleware<IdentityCheckerMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
