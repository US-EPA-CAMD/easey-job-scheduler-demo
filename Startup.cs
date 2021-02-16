using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EPA.CAMPD.Scheduler
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
            //services.AddControllers();

            // base configuration from appsettings.json
            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));            

            services.AddQuartz(q =>
            {
                // base quartz scheduler, job and trigger configuration

                // handy when part of cluster or you want to otherwise identify multiple schedulers
                q.SchedulerId = "Main";

                // we could leave DI configuration intact and then jobs need to have public no-arg constructor
                // the MS DI is expected to produce transient job instances 
                q.UseMicrosoftDependencyInjectionJobFactory(options =>
                {
                    // if we don't have the job in DI, allow fallback to configure via default constructor
                    options.AllowDefaultConstructor = true;
                });
    
                // or 
                // q.UseMicrosoftDependencyInjectionScopedJobFactory();                

                // these are the defaults
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => {
                    tp.MaxConcurrency = 10;
                });

                q.ScheduleJob<MainJob>(trigger => trigger
                    .WithIdentity("MainJob")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithRepeatCount(0)
                    )
                    //.WithCronSchedule("0 0 * ? * *")
                    //.StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
                    //.WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
                    .WithDescription("my awesome trigger configured for a job with single call")
                );
            });

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            // app.UseHttpsRedirection();

            // app.UseRouting();

            // app.UseAuthorization();

            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapControllers();
            // });
        }
    }
}
