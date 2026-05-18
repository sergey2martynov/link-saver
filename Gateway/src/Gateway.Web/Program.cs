using System.Text;
using Gateway.Web.Auth;
using Gateway.Web.Transformers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<UserServiceClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:UserService"]!));

builder.Services.AddSingleton<TelegramAuthService>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<UserIdHeaderTransform>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapReverseProxy();

app.Run();
