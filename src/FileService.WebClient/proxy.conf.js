const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/identity",
      "/bff",
      "/signin-oidc",
      "signout-callback-oidc",
      "/signout-callback-oidc"
    ],
    target: process.env["BFF__Url"],
    secure: false
  }
]

module.exports = PROXY_CONFIG;