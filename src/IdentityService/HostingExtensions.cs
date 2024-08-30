using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MesaProject.IdentityService.Services;
using IdentityService;

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