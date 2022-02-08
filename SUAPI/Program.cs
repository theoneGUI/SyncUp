using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;

namespace SyncUpAPI {
    public class Program {
        public static void Main(string[] args)
        {
            Thread ticker = new Thread(Universe.DoTick);
            ticker.Start();
            Console.CancelKeyPress += delegate
            {
                Logg.printInfo("Ctrl-C pressed...");
                Logg.printInfo("Shutting down server and exiting program");
                Logg.printInfo("Waiting for ticker thread to join...");
                Universe.END_OF_WORLD = true;
                ticker.Join();
                Logg.printInfo("Press any key to close this window");
                Console.ReadKey();
            };
            Logg.printInfo("Starting up server...");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            Logg.printInfo("Using server version: " + version);
            int port = 8080;
            try
            {
                Logg.printInfo($"Attempting to bind to port {port}");
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseUrls($"https://*:{port}")
                    .Build();
                host.Run();
            }
            catch (Exception e)
            {
                Logg.printError("The program has crashed");
                Logg.printErrorDetails(e.Message);
                Logg.printInfo("Press any key to close this window");
                Console.ReadKey();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
