using Backend_Pixel_Crawler;
using Backend_Pixel_Crawler.Database;
using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Managers;
using Backend_Pixel_Crawler.Network.Transport.TCP;
using Backend_Pixel_Crawler.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<TCPWorker>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, HashPasswordService>();
builder.Services.AddScoped<IUserAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddSingleton<ITokenCacheService, RedisTokenCacheService>();
builder.Services.AddScoped<TCPSessionManager>();
builder.Services.AddScoped<LobbyManager>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "backend-pixel-crawler-dist-cache";
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5009); // HTTP endpoint
    options.ListenAnyIP(7206, listenOptions =>
    {
        listenOptions.UseHttps("/etc/haproxy/certs/pixelcrawler.online.crt", "/etc/haproxy/certs/pixelcrawler.online.key");
    });
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization(); 

app.MapControllers();

app.Run();
