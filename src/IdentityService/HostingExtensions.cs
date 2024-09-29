using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MesaProject.IdentityService.Services;
using IdentityService;
using Google.Protobuf.WellKnownTypes;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Services;
using IdentityService.Options;

namespace MesaProject.IdentityService
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.Services.AddControllers();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("identitydb"), options =>
                    options.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)
                )
            );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<EmailSenderOptions>(builder.Configuration.GetSection("Mail"));

            builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

            builder.Services.AddSingleton<ICorsPolicyService>((container) =>
            {
                var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();

                return new DefaultCorsPolicyService(logger)
                {
                    AllowedOrigins = { "https://localhost:4200", builder.Configuration["FileService:Url"], builder.Configuration["Bff:Url"] }
                };
            });
            
            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;

                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.GetClients(builder.Configuration))
                .AddAspNetIdentity<ApplicationUser>();

            var authenticationSection = builder.Configuration.GetRequiredSection("Authentication");

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    var googleSection = authenticationSection.GetRequiredSection("Google");
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = googleSection["ClientId"]
                        ?? throw new KeyNotFoundException($"{nameof(googleSection)}.ClientId");
                    options.ClientSecret = googleSection["ClientSecret"]
                        ?? throw new KeyNotFoundException($"{nameof(googleSection)}.ClientSecret");
                })
                .AddMicrosoftAccount(options =>
                {
                    var microsoftSection = authenticationSection.GetRequiredSection("Microsoft");
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = microsoftSection["ClientId"]
                        ?? throw new KeyNotFoundException($"{nameof(microsoftSection)}.ClientId");
                    options.ClientSecret = microsoftSection["ClientSecret"]
                        ?? throw new KeyNotFoundException($"{nameof(microsoftSection)}.ClientSecret");
                });

            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.AddLocalApiAuthentication();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost4200", policy =>
                {
                    policy.WithOrigins("https://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.MapControllers();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowLocalhost4200");
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("Content-Security-Policy", "script-src 'sha256-fa5rxHhZ799izGRP38+h4ud5QXNT0SFaFlh4eqDumBI=' https://localhost:5001;");
            //    await next();
            //});
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapGrpcService<Services.IdentityService>();

            app.MapRazorPages()
                .RequireAuthorization();

            return app;
        }
    }
}