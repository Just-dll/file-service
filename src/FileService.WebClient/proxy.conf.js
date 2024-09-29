/*module.exports = {
  "/api": {
    target:
      process.env["FileService__Url"],
    secure: process.env["NODE_ENV"] !== "development",
    
  },
  "/identity": {
    target:
      process.env["Identity__Url"],
      secure: process.env["NODE_ENV"] !== "development",
      pathRewrite: {
        "^/identity": "",
      },
      changeOrigin : true,
  }
};
*/
const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/identity",
      "/bff",
      "/signin-oidc",
      "signout-callback-oidc",
    ],
    //target: process.env["BFF__Url"],
    target: "https://localhost:7082",
    secure: false
  }
]

module.exports = PROXY_CONFIG;