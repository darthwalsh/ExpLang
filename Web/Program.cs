using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web
{
  public class Program
  {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((context, logging) => {
              // When trying to debug a request using the debugger, the console output is redundant
              logging.ClearProviders(); // Remove the Debug provider then add the rest back

              logging.AddConfiguration(context.Configuration.GetSection("Logging"));
              logging.AddConsole();
              logging.AddEventSourceLogger();
            });
  }
}
