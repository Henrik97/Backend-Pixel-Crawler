using Backend_Pixel_Crawler;
using Backend_Pixel_Crawler.Database;
using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Network.Transport.TCP;
using Backend_Pixel_Crawler.Services;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<TCPWorker>();


builder.Services.AddScoped<IPasswordHasher, HashPasswordService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthorization(); 

app.MapControllers();

app.UseCors();
app.Run();
