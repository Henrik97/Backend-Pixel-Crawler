using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend_Pixel_Crawler

{
    public class StartupTry
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR(); // Add SignalR services to the service collection
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Map SignalR hub endpoint
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHubTry>("/game-hub");
            });
        }
    }
}
