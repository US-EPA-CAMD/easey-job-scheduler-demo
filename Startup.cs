using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

using Epa.Camd.Easey.JobScheduler.Jobs;
using Epa.Camd.Easey.RulesApi.Models;

namespace Epa.Camd.Easey.JobScheduler
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
            //services.AddScoped<NpgSqlContext>();

            services.AddDbContext<NpgSqlContext>(options => 
                options.UseNpgsql(Configuration.GetConnectionString("Epa.Camd.Postgres"))
            );

            // base configuration from appsettings.json
            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));

            services.AddQuartz(q =>
            {
                // base quartz scheduler, job and trigger configuration

                // handy when part of cluster or you want to otherwise identify multiple schedulers
                q.SchedulerId = "AUTO";

                // we could leave DI configuration intact and then jobs need to have public no-arg constructor
                // the MS DI is expected to produce transient job instances 
                //q.UseMicrosoftDependencyInjectionJobFactory(options =>
                //{
                //    // if we don't have the job in DI, allow fallback to configure via default constructor
                //    options.AllowDefaultConstructor = true;
                //});
    
                // or 
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                // these are the defaults
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => {
                    tp.MaxConcurrency = 25;
                });

                // q.ScheduleJob<MainJob>(trigger => trigger
                //     .WithIdentity("MainJob")
                //     .StartNow()
                //     .WithSimpleSchedule(x => x
                //         .WithRepeatCount(0)
                //     )
                // );

                q.ScheduleJob<CheckEngine>(trigger => trigger
                    .WithIdentity("Check Engine")
                    .WithDailyTimeIntervalSchedule(x => x.WithInterval(1, IntervalUnit.Minute))
                    .WithDescription("Check Engine will poll submission queue every 60 seconds and schedule submission processes.")
                );                
            });

            services.AddTransient<MonitorPlanEvaluation>();

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
