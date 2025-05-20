using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 🔧 AWS Configuration
// ========================
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// ========================
// 🔐 Authentication: Cognito + Cookies
// ========================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var cognitoDomain = builder.Configuration["Cognito:Domain"];
    var region = builder.Configuration["AWSRegion"];
    var clientId = builder.Configuration["Cognito:ClientId"];

    options.Authority = $"https://{cognitoDomain}.auth.{region}.amazoncognito.com";
    options.ClientId = clientId;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.CallbackPath = "/signin-oidc";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name", // Optional: what appears in User.Identity.Name
        RoleClaimType = "cognito:groups" // Enables group-based policies
    };
    options.Scope.Add("openid");
    options.Scope.Add("email");
});

// ========================
// 🛡️ Authorization Policies
// ========================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("cognito:groups", "Admins");
    });
});

// ========================
// 📦 MVC
// ========================
builder.Services.AddControllersWithViews();

// ========================
// 🚀 App pipeline
// ========================
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}");

app.Run();
