using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

using Epa.Camd.Easey.JobScheduler.Jobs;
using Epa.Camd.Easey.RulesApi.Models;
using Npgsql;
using NpgsqlTypes;
using Quartz.Impl.AdoJobStore.Common;
using System;

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

            string k = Configuration.GetConnectionString("Epa.Camd.Postgres");

            Console.WriteLine(k);

            // base configuration from appsettings.json
            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));

            services.AddQuartz(q =>
            {



                DbProvider.RegisterDbMetadata("Npgsql-30", new DbMetadata
                {
                    AssemblyName = typeof(NpgsqlConnection).Assembly.FullName,
                    BindByName = true,
                    ConnectionType = typeof(NpgsqlConnection),
                    CommandType = typeof(NpgsqlCommand),
                    ParameterType = typeof(NpgsqlParameter),
                    ParameterDbType = typeof(NpgsqlDbType),
                    ParameterDbTypePropertyName = "NpgsqlDbType",
                    ParameterNamePrefix = ":",
                    ExceptionType = typeof(NpgsqlException),
                    UseParameterNamePrefixInParameterCollection = true
                });
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
                //  q.UseSimpleTypeLoader();
                q.SetProperty("quartz.serializer.type", "binary");
                q.SetProperty("quartz.scheduler.instanceName", "QuartzWithCore");
                q.SetProperty("quartz.scheduler.instanceId", "QuartzWithCore");
                q.SetProperty("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                q.SetProperty("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                q.SetProperty("quartz.jobStore.dataSource", "default");
                q.SetProperty("quartz.dataSource.default.connectionString", "server=localhost;port=15210;user id=uImcwuf4K9dyaxeL;password=f7GTHc5O3Tvy8lp9njrG3BcLU;database=cgawsbrokerprodr97macy19l;pooling=true");
                q.SetProperty("quartz.dataSource.default.provider", "Npgsql-30");
                q.SetProperty("quartz.jobStore.tablePrefix", "camdaux.qrtz_");

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

                Console.WriteLine(Configuration.GetConnectionString("Epa.Camd.Postgres"));

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



