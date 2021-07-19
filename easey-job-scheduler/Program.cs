using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using SilkierQuartz;

namespace Epa.Camd.Easey.JobScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureSilkierQuartzHost();
    }
}
