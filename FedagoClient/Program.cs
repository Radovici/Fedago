using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DataBazrPeer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    //config.AddInMemoryCollection(arrayDict);
                    //config.AddJsonFile("access.json", optional: false, reloadOnChange: false);
                    //config.AddEFConfiguration(options => options.UseInMemoryDatabase("InMemoryDb"));
                    //config.AddCommandLine(args);
                })
                .UseStartup<Startup>();
    }
}
