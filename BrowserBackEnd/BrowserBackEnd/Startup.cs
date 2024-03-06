using BrowserBackEnd.Hubs;
using BrowserBackEnd.Midelwares;
using BrowserBackEnd.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BrowserBackEnd", Version = "v1" });
            });

            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IStoreService, StoreService>();

            services.AddSignalR(conf =>
            {
                conf.MaximumReceiveMessageSize = null;
            }).AddMessagePackProtocol();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            var useRawWbsockets = true;
            if(useRawWbsockets)
            {
                app.UseWebSockets();
                app.UseMiddleware<WebSocketMiddleware>();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BrowserBackEnd v1"));
            }

            app.UseCors(x => x
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials());

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                if (!useRawWbsockets)
                {
                    endpoints.MapHub<WebSocketUploadHub>("/hub/upload");
                }
            });
           
        }
    }
}
