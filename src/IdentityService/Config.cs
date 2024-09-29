using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
                new ApiScope("department", ["department.id"]),
                new ApiScope("api")
            };

        public static IEnumerable<Client> GetClients(IConfiguration config) =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44300/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },

                new Client
                {
                    ClientId = "mobile",
                    ClientSecrets = { new Secret("19C2943A-A12C-30A1-3901-B1C2391E239A".Sha256())},

                    AllowedGrantTypes = GrantTypes.Hybrid,
                },

                new Client
                {
                    ClientId = "fileService",
                    ClientSecrets = { new Secret("3B7E82F9-C45B-11EC-952C-0242AC120002".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { $"{config["FileService:Url"]}/signin-oidc", $"{config["Bff:Url"]}/signin-oidc", $"{config["WebClient:Url"]}/signin-oidc" },
                    FrontChannelLogoutUri = $"{config["WebClient:Url"]}/signout-oidc",
                    PostLogoutRedirectUris = { $"{config["WebClient:Url"]}/signout-callback-oidc" },

                    AllowOfflineAccess = true,

                    AllowedScopes = { "openid", "profile", "email", "api", "offline_access" }
                }
            };
    }
}
