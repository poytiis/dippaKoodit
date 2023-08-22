using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BrowserBackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     config.AddJsonFile("appsettings.Secrets.json",
                         optional: true,
                         reloadOnChange: true);
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var useHTTPS = Environment.GetEnvironmentVariable("USE_HTTPS");
                    if(useHTTPS != "true")
                    {
                        webBuilder.ConfigureKestrel(opt => {
                            opt.ListenAnyIP(5000);

                            opt.ListenAnyIP(5001, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                listenOptions.UseHttps();
                            });
                        });
                    }
                    else
                    {
                        webBuilder.ConfigureKestrel(opt => {
                            opt.ListenAnyIP(5000);                         
                        });
                    }
                    webBuilder.UseStartup<Startup>();
                })
            ;
    }
}
