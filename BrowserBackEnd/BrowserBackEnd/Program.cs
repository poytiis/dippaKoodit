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
using System.Runtime.InteropServices;
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

                    var certPath = "C:\\Users\\OWNER\\dippa\\dippa-teemup\\BrowserBackEnd\\certificate\\localhost.pfx";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        certPath = "/home/poytiis/certs/dippa.test.pfx";
                    }

                    webBuilder.ConfigureKestrel(opt =>
                    {
                        opt.ListenAnyIP(5000);

                        opt.ListenAnyIP(5001, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                            listenOptions.UseHttps(certPath);
                        });
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
