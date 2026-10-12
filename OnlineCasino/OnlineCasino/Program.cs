using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using OnlineCasino.Data;
using OnlineCasino.Data.Seed;

using OnlineCasino.Mappings;
 
using OnlineCasino.Middlewares;

using OnlineCasino.Services;
  
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=/app/data/casino.db"));
 
builder.Services.AddAutoMapper(
    cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"],
    typeof(MappingProfile));
 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ISlotsService, SlotsService>();
builder.Services.AddScoped<IRouletteService, RouletteService>();
builder.Services.AddScoped<IBlackjackService, BlackjackService>();
builder.Services.AddScoped<IGameHistoryService, GameHistoryService>();
 
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-for-demo-min-32-chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "online-casino",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "online-casino-app",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
         
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
var app = builder.Build();
 
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();

public partial class Program { }