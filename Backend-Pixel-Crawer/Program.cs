using Backend_Pixel_Crawler;
using Backend_Pixel_Crawler.Network.Transport.TCP;
using SharedLibrary;





var builder = WebApplication.CreateBuilder(args);

//Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<TCPWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthorization(); 

app.MapControllers();

app.Run();
